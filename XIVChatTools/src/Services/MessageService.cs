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

namespace XIVChatTools.Services;

[PluginInterface]
public class MessageService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly List<ChatEntry> _chatEntries;

    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;
    
    private string directoryPath => Configuration.MessageLog_FilePath;
    private string fullFilePath => Path.Combine(Configuration.MessageLog_FilePath, Configuration.MessageLog_FileName);

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    private readonly AdvancedDebugLogger? advancedDebugLogger = null;

    public MessageService(Plugin plugin)
    {
        _plugin = plugin;
        _chatEntries = TryRestoreChatEntries();

        if (PluginInterface.IsDev)
        {
            Logger.Debug("Chat Tools is running in development mode.");
            advancedDebugLogger = new AdvancedDebugLogger(plugin);
        }
    }

    private List<ChatEntry> TryRestoreChatEntries()
    {
        if (Directory.Exists(directoryPath) == false)
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not create new chat log directory.");
            }
        }

        if (File.Exists(fullFilePath) == false)
        {
            try
            {
                File.WriteAllLines(fullFilePath, ["[", "", "]"]);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not create new chat log file.");
            }
        }

        if (_plugin.Configuration.MessageLog_PreserveOnLogout && File.Exists(fullFilePath))
        {
            try
            {
                var FileResult = File.ReadAllText(fullFilePath);
                var options = new JsonSerializerOptions
                {
                    IncludeFields = true
                };

                List<ChatEntry> ChatEntries = JsonSerializer.Deserialize<List<ChatEntry>>(FileResult, options) ?? [];

                if (_plugin.Configuration.MessageLog_DeleteOldMessages)
                {
                    ChatEntries = ChatEntries
                      .Where(t => (DateTime.Now - t.DateSent).TotalDays < _plugin.Configuration.MessageLog_DaysToKeepOldMessages)
                      .ToList();
                }

                return ChatEntries;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load Chat Logs.");
            }
        }
        else
        {
            Logger.Information("Log file not found.");
        }

        return [];
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

        var parsedSenderName = ParseSenderName(type, sender);

        if (Configuration.DebugLogging)
        {
            ChatDevLogging(type, timestamp, sender, message, parsedSenderName);
        }

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

        AddChatMessage(new Models.ChatEntry()
        {
            ChatType = type,
            OwnerId = Helpers.PlayerCharacter.Name,
            Message = message.TextValue,
            Timestamp = timestamp,
            SenderName = parsedSenderName
        });
    }

    private string GetPlayerName()
    {
        if (ClientState.LocalPlayer != null)
        {
            return ClientState.LocalPlayer.Name.TextValue;
        }
        else
        {
            return "";
        }
    }

    internal List<ChatEntry> GetAllMessages()
    {
        return this._chatEntries
          .Where(t => t.OwnerId == Helpers.PlayerCharacter.Name)
          .ToList();
    }

    internal List<ChatEntry> GetMessagesForFocusTarget()
    {
        PlayerIdentifier? focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

        if (focusTarget == null)
        {
            return [];
        }

        return this._chatEntries
          .Where(t => t.OwnerId == Helpers.PlayerCharacter.Name)
          .Where(t => t.SenderName == focusTarget.Name || t.SenderName.StartsWith(focusTarget.Name))
          .ToList();
    }

    internal List<ChatEntry> GetMessagesByPlayerNames(List<string> names)
    {
        return this._chatEntries
          .Where(t => t.OwnerId == Helpers.PlayerCharacter.Name)
          .Where(t => names.Any(name => t.SenderName == name || t.SenderName.StartsWith(name)))
          .ToList();
    }

    internal List<ChatEntry> SearchMessages(string searchText)
    {
        if (searchText == string.Empty)
        {
            return this._chatEntries.ToList();
        }

        return this._chatEntries
            .Where(t =>
                t.Message.ToLower().Contains(searchText.ToLower()) ||
                t.SenderName.ToLower().Contains(searchText.ToLower()))
            .ToList();
    }

    private void AddChatMessage(ChatEntry chatEntry)
    {
        this._chatEntries.Add(chatEntry);

        if (_plugin.Configuration.MessageLog_PreserveOnLogout)
        {
            PersistMessages();
        }
    }

    internal void ClearMessageHistory()
    {
        this._chatEntries.Clear();
    }

    private async void PersistMessages()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };
            var jsonData = JsonSerializer.Serialize(this._chatEntries.ToArray(), options);

            await File.WriteAllTextAsync(fullFilePath, jsonData);
        }
        catch (Exception ex)
        {
            Logger.Error("An error has occurred while trying to save chat history:" + ex.Message);
        }
    }

    private string ParseSenderName(XivChatType type, SeString sender)
    {
        Payload? payload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

        if (payload is PlayerPayload playerPayload)
        {
            return playerPayload.PlayerName;
        }

        if (type == XivChatType.StandardEmote)
        {
            payload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.RawText);

            if (payload is TextPayload textPayload && textPayload.Text != null)
            {
                return textPayload.Text;
            }
        }
        else
        {
            return Helpers.PlayerCharacter.Name;
        }

        return "N/A|BadType";
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
