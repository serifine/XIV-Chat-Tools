using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.IoC;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Game.Text.SeStringHandling;
using XIVChatTools.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using XIVChatTools.Database;
using XIVChatTools.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace XIVChatTools.Services;

internal delegate void MessageAddedHandler(PlayerIdentifier sender, Message message);

[PluginInterface]
public class MessageService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly KeywordWatcher keywordWatcher;
    private readonly AdvancedDebugLogger? advancedDebugLogger = null;

    private ChatToolsDbContext _dbContext => _plugin.DbContext;
    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;
    private IDalamudPluginInterface PluginInterface => Plugin.PluginInterface;
    private IClientState ClientState => Plugin.ClientState;
    private IPluginLog Logger => Plugin.Logger;

    internal event MessageAddedHandler? MessageAdded;

    public MessageService(Plugin plugin)
    {
        _plugin = plugin;
        keywordWatcher = new KeywordWatcher(plugin);

        MessageAdded += OnMessageAdded;

        if (PluginInterface.IsDev)
        {
            Logger.Debug("Chat Tools is running in development mode.");
            advancedDebugLogger = new AdvancedDebugLogger(plugin);
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
        keywordWatcher.HandleMessage(message.MessageContents);
    }

    internal void HandleChatMessage(XivChatType type, int timestamp, SeString sender, SeString message)
    {
        if (Constants.ChatTypes.IsSupportedChatType(type) == false || !Configuration.ActiveChannels.Any(t => t == type))
        {
            return;
        }

        var parsedSender = ParseSender(type, sender);

        var newMessage = new Message()
        {
            Timestamp = DateTime.Now,
            MessageContents = message.TextValue,
            ChatType = type,
            OwningPlayer = _dbContext.GetLoggedInPlayer(),
            SenderName = parsedSender.Name,
            SenderWorld = parsedSender.World
        };

        try
        {
            _dbContext.Messages.Add(newMessage);
            _dbContext.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            Logger.Error($"Error saving message to database: {ex.Message}");
        }

        TriggerMessageAddedEvent(parsedSender, newMessage);

        if (Configuration.DebugLogging)
        {
            ChatDevLogging(type, timestamp, sender, message, parsedSender.Name + "|" + parsedSender.World);
        }
    }

    internal List<Message> GetAllMessages()
    {
        return this._dbContext.Messages
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

        return this._dbContext.Messages
            .Where(t => t.OwningPlayer.Name == Helpers.PlayerCharacter.Name)
            .Where(t => t.SenderName == player.Name && t.SenderWorld == player.World)
            .Where(t => t.Timestamp >= DateTime.Now.AddDays(-14))
            .OrderBy(t => t.Timestamp)
            .AsNoTracking()
            .ToList();
    }

    internal List<Message> GetMessagesForPlayers(List<PlayerIdentifier> players)
    {
        List<string> playerIdentifiers = players.Select(t => $"{t.Name}@{t.World}").ToList();

        return this._dbContext.Messages
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
            return this._dbContext.Messages.ToList();
        }

        return this._dbContext.Messages
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

    private void ChatDevLogging(XivChatType type, int timestamp, SeString sender, SeString message, string parsedSenderName)
    {
        if (parsedSenderName == "N/A|BadType")
        {
            Logger.Error("NEW CHAT MESSAGE: UNABLE TO PARSE NAME");
            Logger.Error("=======================================================");
            Logger.Error("Message Type: " + type.ToString());
            Logger.Error("Raw Sender: " + sender.TextValue);
            Logger.Error("Parsed Sender: " + parsedSenderName);
        }
        else
        {
            Logger.Debug("NEW CHAT MESSAGE RECEIVED");
            Logger.Debug("=======================================================");
            Logger.Debug("Message Type: " + type.ToString());
            Logger.Debug("Raw Sender: " + sender.TextValue);
            Logger.Debug("Parsed Sender: " + parsedSenderName);
        }


        if (PluginInterface.IsDev && advancedDebugLogger != null)
        {
            SeString modifiedSender = sender;

            advancedDebugLogger.AddNewMessage(new()
            {
                ChatType = type.ToString(),
                Timestamp = timestamp,
                TextValue = message.TextValue,
                ParsedSender = parsedSenderName,
                Sender = sender,
                Message = message
            });
        }
    }
}
