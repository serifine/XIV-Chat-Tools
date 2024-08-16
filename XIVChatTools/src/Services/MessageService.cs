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

namespace XIVChatTools.Services;

[PluginInterface]
public class MessageService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly List<ChatEntry> _chatEntries;

    private string directoryPath => _plugin.Configuration.MessageLog_FilePath;
    private string fullFilePath => Path.Combine(_plugin.Configuration.MessageLog_FilePath, _plugin.Configuration.MessageLog_FileName);

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    public MessageService(Plugin plugin)
    {
        _plugin = plugin;
        _chatEntries = TryRestoreChatEntries();
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

    public string GetPlayerName()
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

    public List<IPlayerCharacter> GetNearbyPlayers()
    {
        return ObjectTable
          .Where(t => t.Name.TextValue != GetPlayerName() && t.ObjectKind == ObjectKind.Player)
          .Cast<IPlayerCharacter>()
          .OrderBy(t => t.Name.TextValue)
          .ToList();
    }

    public List<ChatEntry> GetAllMessages()
    {
        return this._chatEntries
          .Where(t => t.OwnerId == GetPlayerName())
          .ToList();
    }

    public List<ChatEntry> GetMessagesForFocusTarget()
    {
        IPlayerCharacter? focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

        if (focusTarget == null)
        {
            return [];
        }

        return this._chatEntries
          .Where(t => t.OwnerId == GetPlayerName())
          .Where(t => t.SenderName == focusTarget.Name.TextValue || t.SenderName.StartsWith(focusTarget.Name.TextValue))
          .ToList();
    }

    public List<ChatEntry> GetMessagesByPlayerNames(List<string> names)
    {
        return this._chatEntries
          .Where(t => t.OwnerId == GetPlayerName())
          .Where(t => names.Any(name => t.SenderName == name || t.SenderName.StartsWith(name)))
          .ToList();
    }

    public List<ChatEntry> SearchMessages(string searchText)
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

    public void AddChatMessage(ChatEntry chatEntry)
    {
        this._chatEntries.Add(chatEntry);

        if (_plugin.Configuration.MessageLog_PreserveOnLogout)
        {
            PersistMessages();
        }
    }

    public void ClearMessageHistory()
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
}
