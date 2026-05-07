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
    private readonly MessageProcessor _messageProcessor;
    private readonly AdvancedDebugLogger? _advancedDebugLogger = null;

    private Configuration Configuration => _plugin.Configuration;
    private ChatToolsDatabase DbContext => _plugin.DbContext;
    private KeywordWatcher KeywordWatcher => _plugin.KeywordWatcher;
    private PluginStateService PluginState => _plugin.PluginState;
    private static IDalamudPluginInterface PluginInterface => Plugin.PluginInterface;
    private static IPluginLog Logger => Plugin.Logger;

    internal event MessageAddedHandler? MessageAdded;

    public MessageService(Plugin plugin)
    {
        _plugin = plugin;
        _messageProcessor = new MessageProcessor(plugin);

        MessageAdded += OnMessageAdded;

        if (PluginInterface.IsDev)
        {
            _advancedDebugLogger = new AdvancedDebugLogger(plugin);
        }
    }

    public void Dispose()
    {

    }

    private void TriggerMessageAddedEvent(Message message)
    {
        PlayerIdentifier sender = new PlayerIdentifier(message.SenderName, message.SenderWorld);
        message = _messageProcessor.SplitMessageContents(message);

        MessageAdded?.Invoke(sender, message);
    }

    private void OnMessageAdded(PlayerIdentifier sender, Message message)
    {
        KeywordWatcher.HandleMessage(message.MessageContents);
    }

    internal void HandleChatMessage(IChatMessage chatMessage) // (XivChatType type, int timestamp, SeString sender, SeString message)
    {
        if (!Constants.ChatTypes.IsSupportedChatType(chatMessage.LogKind) || Configuration.ActiveChannels.All(t => t != chatMessage.LogKind))
        {
            return;
        }

        Message? newMessage = _messageProcessor.ProcessMessage(chatMessage);

        if (newMessage == null)
            return;

        try
        {
            DbContext.AddMessage(newMessage);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error saving message to database: {ex.Message}");
        }


        TriggerMessageAddedEvent(newMessage);

        if (Configuration.DebugLogging)
        {
            ChatDevLogging(chatMessage, newMessage);
        }
    }

    internal List<Message> GetAllMessages()
    {
        return DbContext
            .GetAllMessages(Helpers.PlayerCharacter.Name)
            .Select(_messageProcessor.SplitMessageContents)
            .ToList();
    }

    internal List<Message> GetMessagesForPlayer(PlayerIdentifier player)
    {
        if (player == null)
        {
            return [];
        }

        return DbContext
            .GetMessagesForPlayer(Helpers.PlayerCharacter.Name, player.Name, player.World)
            .Select(_messageProcessor.SplitMessageContents)
            .ToList(); ;
    }

    internal List<Message> GetMessagesForPlayers(List<PlayerIdentifier> players)
    {
        var playerKeys = players.Select(t => $"{t.Name}@{t.World}").ToList();
        return DbContext
            .GetMessagesForPlayers(Helpers.PlayerCharacter.Name, playerKeys)
            .Select(_messageProcessor.SplitMessageContents)
            .ToList(); ;
    }

    internal List<Message> SearchMessages(string searchText)
    {
        IEnumerable<Message> messageResults;

        if (searchText == string.Empty)
        {
            messageResults = DbContext.GetAllMessages(Helpers.PlayerCharacter.Name);
        }
        else
        {
            messageResults = DbContext.SearchMessages(Helpers.PlayerCharacter.Name, searchText);
        }

        return messageResults
            .Select(_messageProcessor.SplitMessageContents)
            .ToList();
    }

    private void ChatDevLogging(IChatMessage rawMessage, Message message) // (XivChatType type, int timestamp, SeString sender, SeString message, string parsedSenderName)
    {
        var parsedSenderName = message.SenderName + "|" + message.SenderWorld;

        if (parsedSenderName == "N/A|BadType")
        {
            Logger.Error("NEW CHAT MESSAGE: UNABLE TO PARSE NAME");
        }
        else
        {
            Logger.Debug("NEW CHAT MESSAGE RECEIVED");
        }

        Logger.Debug("=======================================================");
        Logger.Debug("Message Type: " + rawMessage.LogKind.ToString());
        Logger.Debug("Raw Sender: " + rawMessage.Sender.TextValue);
        Logger.Debug("Parsed Sender: " + parsedSenderName);


        if (!PluginInterface.IsDev || _advancedDebugLogger == null) return;

        var modifiedSender = rawMessage.Sender;

        _advancedDebugLogger.AddNewMessage(rawMessage, parsedSenderName);
    }
}

/// <summary>
/// Helper class for processing chat messages and their parts.
/// </summary>
internal class MessageProcessor(Plugin plugin)
{
    private ChatToolsDatabase DbContext => plugin.DbContext;
    private KeywordWatcher KeywordWatcher => plugin.KeywordWatcher;
    private IPluginLog Logger => Plugin.Logger;

    internal Message? ProcessMessage(IChatMessage chatMessage)
    {
        var parsedSender = ParseSender(chatMessage.LogKind, chatMessage.Sender);

        if (parsedSender == null)
        {
            Logger.Error("Unable to parse sender for incoming message. Message will be ignored.");
            return null;
        }

        var messageParts = ProcessMessagePayloads(chatMessage);

        return new Message()
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
    }

    /// <summary>
    /// Splits message contents that are purely text into separate parts for each word. This allows for
    /// keyword highlighting and formatting to be done on individual words instead of entire message blobs.
    /// </summary>
    internal Message SplitMessageContents(Message message)
    {
        List<IMessagePart> splitContents = new List<IMessagePart>();

        foreach (var messagePart in message.MessageContents)
        {
            if (messagePart is MessagePart textPart)
            {
                foreach (string word in textPart.Text.Trim().Split(' '))
                {
                    var part = new MessagePart(word);

                    if (KeywordWatcher.IsWatchedTerm(word))
                    {
                        part.Watched = true;
                    }

                    splitContents.Add(part);
                }
            }
            else
            {
                splitContents.Add(messagePart);
            }
        }

        message.MessageContents = splitContents;

        return message;
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

    private List<IMessagePart> ProcessMessagePayloads(IChatMessage chatMessage)
    {
        List<IMessagePart> messageParts = new List<IMessagePart>();

        foreach (var payload in chatMessage.Message.Payloads)
        {
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
}