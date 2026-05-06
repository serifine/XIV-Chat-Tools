using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using XIVChatTools.DB;
using XIVChatTools.DB.Models;
using XIVChatTools.Models;

namespace XIVChatTools.Services;

internal delegate void MessageAddedHandler(PlayerIdentifier sender, Message message);

[PluginInterface]
public class MessageService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly KeywordWatcher _keywordWatcher;
    private readonly AdvancedDebugLogger? _advancedDebugLogger = null;

    private ChatToolsDatabase DbContext => _plugin.DbContext;
    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;
    private static IDalamudPluginInterface PluginInterface => Plugin.PluginInterface;
    private static IPluginLog Logger => Plugin.Logger;

    internal event MessageAddedHandler? MessageAdded;

    public MessageService(Plugin plugin)
    {
        _plugin = plugin;
        _keywordWatcher = new KeywordWatcher(plugin);

        MessageAdded += OnMessageAdded;

        if (PluginInterface.IsDev)
        {
            _advancedDebugLogger = new AdvancedDebugLogger(plugin);
        }
    }

    public void Dispose()
    {

    }

    private void TriggerMessageAddedEvent(PlayerIdentifier sender, Message message)
    {
        MessageAdded?.Invoke(sender, message);
    }

    private void OnMessageAdded(PlayerIdentifier sender, Message message)
    {
        _keywordWatcher.HandleMessage(message.MessageContents);
    }

    internal void HandleChatMessage(IChatMessage chatMessage) // (XivChatType type, int timestamp, SeString sender, SeString message)
    {
        if (!Constants.ChatTypes.IsSupportedChatType(chatMessage.LogKind) || Configuration.ActiveChannels.All(t => t != chatMessage.LogKind))
        {
            return;
        }

        var parsedSender = ParseSender(chatMessage.LogKind, chatMessage.Sender);

        if (parsedSender == null)
        {
            Logger.Error("Unable to parse sender for incoming message. Message will be ignored.");
            return;
        }

        var messageParts = MessageProcessor.ProcessMessagePayload(chatMessage);

        var newMessage = new Message()
        {
            Timestamp = DateTime.Now,
            MessageContents = messageParts,
            ChatType = chatMessage.LogKind,
            OwningPlayer = DbContext.GetLoggedInPlayer(),
            OwningPlayerName = parsedSender.Name,
            OwningPlayerWorld = parsedSender.World,
            SenderName = parsedSender.Name,
            SenderWorld = parsedSender.World
        };

        try
        {
            DbContext.AddMessage(newMessage);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error saving message to database: {ex.Message}");
        }

        TriggerMessageAddedEvent(parsedSender, newMessage);

        if (Configuration.DebugLogging)
        {
            ChatDevLogging(chatMessage, parsedSender);
        }
    }

    internal List<Message> GetAllMessages()
    {
        return DbContext.GetAllMessages(Helpers.PlayerCharacter.Name);
    }

    internal List<Message> GetMessagesForPlayer(PlayerIdentifier player)
    {
        if (player == null)
        {
            return [];
        }

        return DbContext.GetMessagesForPlayer(Helpers.PlayerCharacter.Name, player.Name, player.World);
    }

    internal List<Message> GetMessagesForPlayers(List<PlayerIdentifier> players)
    {
        var playerKeys = players.Select(t => $"{t.Name}@{t.World}").ToList();
        return DbContext.GetMessagesForPlayers(Helpers.PlayerCharacter.Name, playerKeys);
    }

    internal List<Message> SearchMessages(string searchText)
    {
        if (searchText == string.Empty)
        {
            return DbContext.GetAllMessages(Helpers.PlayerCharacter.Name);
        }

        return DbContext.SearchMessages(Helpers.PlayerCharacter.Name, searchText);
    }

    private PlayerIdentifier? ParseSender(XivChatType type, SeString sender)
    {
        Payload? payload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

        if (payload is PlayerPayload playerPayload)
        {
            return new PlayerIdentifier(playerPayload);
        }

        if (type == XivChatType.StandardEmote)
        {
            payload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.RawText);

            if (payload is TextPayload textPayload && textPayload.Text != null)
            {
                var result = Helpers.NearbyPlayers.SearchForPlayerByName(textPayload.Text);

                if (result != null)
                {
                    return new PlayerIdentifier(result);
                }
            }
        }

        return Helpers.PlayerCharacter.GetPlayerIdentifier();
    }

    private void ChatDevLogging(IChatMessage chatMessage, PlayerIdentifier parsedSender) // (XivChatType type, int timestamp, SeString sender, SeString message, string parsedSenderName)
    {
        var parsedSenderName = parsedSender.Name + "|" + parsedSender.World;
        
        if (parsedSenderName == "N/A|BadType") {
            Logger.Error("NEW CHAT MESSAGE: UNABLE TO PARSE NAME");
        } else {
            Logger.Debug("NEW CHAT MESSAGE RECEIVED");
        }
        
        Logger.Debug("=======================================================");
        Logger.Debug("Message Type: " + chatMessage.LogKind.ToString());
        Logger.Debug("Raw Sender: " + chatMessage.Sender.TextValue);
        Logger.Debug("Parsed Sender: " + parsedSenderName);


        if (!PluginInterface.IsDev || _advancedDebugLogger == null) return;
        
        var modifiedSender = chatMessage.Sender;

        _advancedDebugLogger.AddNewMessage(chatMessage, parsedSenderName);
    }
}

public class MessageProcessor
{
    public static List<IMessagePart> ProcessMessagePayload(IChatMessage chatMessage)
    {
        List<IMessagePart> messageParts = new List<IMessagePart>();

        foreach (var payload in chatMessage.Message.Payloads) {
            if (payload is IconPayload iconPayload)
            {
                messageParts.Add(new IconMessagePart(iconPayload.Icon));
            }

            if (payload is AutoTranslatePayload autoTranslatePayload)
            {
                messageParts.Add(new AutoTranslateMessagePart(autoTranslatePayload.Text));
            }

            if (payload is TextPayload textPayload)
            {
                if (textPayload.Text == null) continue;

                messageParts.Add(new MessagePart(textPayload.Text));
            }
        }
        
        return messageParts; 
    }

    // public static void ProcessMessage(IChatMessage chatMessage)
    // {
    //     Plugin.Instance.MessageService.HandleChatMessage(chatMessage);
    // }
}