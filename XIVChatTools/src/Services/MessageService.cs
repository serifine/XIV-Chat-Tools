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

namespace XIVChatTools.Services;

[PluginInterface]
public class MessageService : IDisposable
{
    private readonly Plugin _plugin;

    private ChatToolsDbContext _dbContext => _plugin.DbContext;
    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;

    private string directoryPath => Configuration.MessageLog_FilePath;
    private string fullFilePath => Path.Combine(Configuration.MessageLog_FilePath, Configuration.MessageLog_FileName);

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    private readonly AdvancedDebugLogger? advancedDebugLogger = null;

    public MessageService(Plugin plugin)
    {
        _plugin = plugin;

        if (PluginInterface.IsDev)
        {
            Logger.Debug("Chat Tools is running in development mode.");
            advancedDebugLogger = new AdvancedDebugLogger(plugin);
        }
    }

    public void Dispose()
    {

    }

    internal void OnChatMessageUnhandled(XivChatType type, int timestamp, SeString sender, SeString message)
    {
        if (Constants.ChatTypes.IsSupportedChatType(type) == false || !Configuration.ActiveChannels.Any(t => t == type))
        {
            return;
        }

        var parsedSender = ParseSender(type, sender);

        if (Configuration.DebugLogging)
        {
            ChatDevLogging(type, timestamp, sender, message, parsedSender.Name + "|" + parsedSender.World);
        }

        //todo: split watcher logic into its own method

        var watchers = Configuration.MessageLog_Watchers.Split(",");
        var messageText = message.TextValue;

        if (Configuration.MessageLog_Watchers.Trim() != "" && watchers.Any(t => messageText.ToLower().Contains(t.ToLower().Trim())))
        {
            try
            {
                UIModule.PlayChatSoundEffect(2);
            }
            catch (Exception ex)
            {
                Logger.Debug("Error playing sound via Dalamud.");
                Logger.Debug(ex.Message);
            }
        }

        _dbContext.Messages.Add(new Message()
        {
            Timestamp = DateTime.Now,
            MessageContents = message.TextValue,
            ChatType = type,
            OwningPlayer = _dbContext.GetLoggedInPlayer(),
            SenderName = parsedSender.Name,
            SenderWorld = parsedSender.World
        });

        _dbContext.SaveChanges();
    }

    internal List<Message> GetAllMessages()
    {
        return this._dbContext.Messages
          .Where(t => t.OwningPlayer.Name == Helpers.PlayerCharacter.Name)
          .ToList();
    }

    internal List<Message> GetMessagesForFocusTarget(PlayerIdentifier focusTarget)
    {
        if (focusTarget == null)
        {
            return [];
        }

        return this._dbContext.Messages
          .Where(t => t.OwningPlayer.Name == Helpers.PlayerCharacter.Name)
          .Where(t => t.SenderName == focusTarget.Name)
          .ToList();
    }

    internal List<Message> GetMessagesByPlayerNames(List<string> names)
    {
        return this._dbContext.Messages
          .Where(t => t.OwningPlayer.Name == Helpers.PlayerCharacter.Name)
          .Where(t => names.Any(name => t.SenderName == name))
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
