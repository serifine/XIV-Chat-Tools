using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.EntityFrameworkCore;
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

    private ChatToolsDbContext DbContext => _plugin.DbContext;
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
            Logger.Debug("Chat Tools is running in development mode.");
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

        var newMessage = new Message()
        {
            Timestamp = DateTime.Now,
            MessageContents = chatMessage.Message.TextValue,
            ChatType = chatMessage.LogKind,
            OwningPlayer = DbContext.GetLoggedInPlayer(),
            SenderName = parsedSender.Name,
            SenderWorld = parsedSender.World
        };

        try
        {
            DbContext.Messages.Add(newMessage);
            DbContext.SaveChanges();
        }
        catch (DbUpdateException ex)
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
        return this.DbContext.Messages
            .Where(t => t.OwningPlayer.Name == Helpers.PlayerCharacter.Name)
            .OrderBy(t => t.Timestamp)
            .AsNoTracking()
            .ToList();
    }

    internal List<Message> GetMessagesForPlayer(PlayerIdentifier player)
    {
        if (player == null)
        {
            return [];
        }

        return this.DbContext.Messages
            .Where(t => t.OwningPlayer.Name == Helpers.PlayerCharacter.Name)
            .Where(t => t.SenderName == player.Name && t.SenderWorld == player.World)
            .Where(t => t.Timestamp >= DateTime.Now.AddDays(-14))
            .OrderBy(t => t.Timestamp)
            .AsNoTracking()
            .ToList();
    }

    internal List<Message> GetMessagesForPlayers(List<PlayerIdentifier> players)
    {
        var playerIdentifiers = players.Select(t => $"{t.Name}@{t.World}").ToList();

        return this.DbContext.Messages
            .Where(t => t.OwningPlayer.Name == Helpers.PlayerCharacter.Name)
            .Where(t => playerIdentifiers.Contains(t.SenderName + "@" + t.SenderWorld))
            .Where(t => t.Timestamp >= DateTime.Now.AddDays(-14))
            .OrderBy(t => t.Timestamp)
            .AsNoTracking()
            .ToList();
    }

    internal List<Message> SearchMessages(string searchText)
    {
        if (searchText == string.Empty)
        {
            return this.DbContext.Messages.ToList();
        }

        return this.DbContext.Messages
            .Where(t =>
                t.MessageContents.ToLower().Contains(searchText.ToLower()) ||
                t.SenderName.ToLower().Contains(searchText.ToLower()))
            .OrderBy(t => t.Timestamp)
            .ToList();
    }

    private PlayerIdentifier ParseSender(XivChatType type, SeString sender)
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

        _advancedDebugLogger.AddNewMessage(new AdvancedDebugEntry
        {
            ChatType = chatMessage.LogKind.ToString(),
            Timestamp = chatMessage.Timestamp,
            TextValue = chatMessage.Message.TextValue,
            ParsedSender = parsedSenderName,
            Sender = chatMessage.Sender,
            Message = chatMessage.Message
        });
    }
}
