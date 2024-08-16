using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using Dalamud.IoC;
using Dalamud;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using XIVChatTools.Services;

namespace XIVChatTools;

public class Plugin : IDalamudPlugin
{
    public static string Name => "Chat Tools";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    internal readonly PluginStateService PluginState;
    internal readonly MessageService MessageService;
    internal readonly WindowManagerService WindowManagerService;
    internal readonly Configuration Configuration;

    private readonly AdvancedDebugLogger? advancedDebugLogger = null;

    private readonly List<string> commandAliases = [
        "/chattools",
        "/ctools",
        "/ct"
    ];

    private readonly List<string> settingsArgumentAliases = [
        "settings",
        "config"
    ];

    public Plugin()
    {
        try
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            // to be removed later
            Configuration.Initialize(PluginInterface);

            PluginState = RegisterService<PluginStateService>();
            MessageService = RegisterService<MessageService>();
            WindowManagerService = RegisterService<WindowManagerService>();

            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;

            PluginInterface.UiBuilder.Draw += OnDrawUI;
            PluginInterface.UiBuilder.OpenMainUi += OnOpenMainUI;
            PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUI;

            ChatGui.ChatMessage += OnChatMessage;

            if (PluginInterface.IsDev)
            {
                Logger.Debug("Chat Tools is running in development mode.");
                advancedDebugLogger = new AdvancedDebugLogger(this);
            }

            foreach (string commandAlias in commandAliases)
            {
                CommandManager.AddHandler(commandAlias, new CommandInfo(OnCommand)
                {
                    HelpMessage = commandAliases.First() == commandAlias ?
                      "Opens the Chat Tools window." : "Alias for /chattools."
                });
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    #region Event Handlers

    private void OnCommand(string command, string args)
    {
        if (settingsArgumentAliases.Contains(args.ToLower()))
        {
            WindowManagerService.SettingsWindow.IsOpen = !WindowManagerService.SettingsWindow.IsOpen;
        }
        else
        {
            OnOpenMainUI();
        }
    }

    private void OnDrawUI()
    {
        WindowManagerService.Draw();

        PostDrawEvents();
    }

    private void OnOpenMainUI()
    {
        WindowManagerService.ToolbarWindow.Toggle();
    }

    private void OnOpenConfigUI()
    {
        WindowManagerService.SettingsWindow.Toggle();
    }

    private void OnLogin()
    {
        WindowManagerService.ToolbarWindow.IsOpen = Configuration.OpenOnLogin;
    }

    private void OnLogout()
    {
        WindowManagerService.CloseAllWindows();
        PluginState.ClearAllFocusTabs();

        if (Configuration.MessageLog_PreserveOnLogout == false)
        {
            MessageService.ClearMessageHistory();
        }
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (Enum.IsDefined(typeof(XivChatType), type) == false || Configuration.AllChannels.Any(t => t.ChatType == type) == false || isHandled || !Configuration.ActiveChannels.Any(t => t == type))
        {
            return;
        }

        var parsedSenderName = ParseSenderName(type, sender);


        if (Configuration.DebugLogging)
        {
            ChatDevLogging(type, timestamp, sender, message, isHandled, parsedSenderName);
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

        MessageService.AddChatMessage(new Models.ChatEntry()
        {
            ChatType = type,
            OwnerId = PluginState.GetPlayerName(),
            Message = message.TextValue,
            Timestamp = timestamp,
            SenderName = parsedSenderName
        });
    }

    #endregion

    public void Dispose()
    {
        ClientState.Login -= OnLogin;
        ClientState.Logout -= OnLogout;

        PluginInterface.UiBuilder.Draw -= OnDrawUI;
        PluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUI;
        PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUI;

        ChatGui.ChatMessage -= OnChatMessage;

        WindowManagerService?.Dispose();

        foreach (string commandAlias in commandAliases)
        {
            if (CommandManager.Commands.Any(t => t.Key == commandAlias))
            {
                CommandManager.RemoveHandler(commandAlias);
            }
        }
    }

    private T RegisterService<T>() where T : class
    {
        try
        {
            var service = PluginInterface.Create<T>(this);

            if (service == null)
            {
                throw new NullReferenceException();
            }

            return service;
        }
        catch (Exception ex)
        {
            Logger.Error($"Fatal Error - Failed to create service: {typeof(T).Name}");
            Logger.Error(ex, ex.Message);

            throw;
        }
    }

    private void PostDrawEvents()
    {

    }

    private string ParseSenderName(XivChatType type, SeString sender)
    {
        Payload? payload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

        if (payload is PlayerPayload playerPayload)
        {
            return playerPayload.PlayerName;
        }

        if (type == XivChatType.CustomEmote || type == XivChatType.StandardEmote)
        {
            payload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.RawText);

            if (payload is TextPayload textPayload && textPayload.Text != null)
            {
                return textPayload.Text;
            }
        }

        if (type == XivChatType.TellOutgoing
         || type == XivChatType.Party
         || type == XivChatType.Say
         || type == XivChatType.CrossLinkShell1
         || type == XivChatType.CrossLinkShell2
         || type == XivChatType.CrossLinkShell3
         || type == XivChatType.CrossLinkShell4
         || type == XivChatType.CrossLinkShell5
         || type == XivChatType.CrossLinkShell6
         || type == XivChatType.CrossLinkShell7
         || type == XivChatType.CrossLinkShell8
         || type == XivChatType.Ls1
         || type == XivChatType.Ls2
         || type == XivChatType.Ls3
         || type == XivChatType.Ls4
         || type == XivChatType.Ls5
         || type == XivChatType.Ls6
         || type == XivChatType.Ls7
         || type == XivChatType.Ls8
        )
        {
            return PluginState.GetPlayerName();
        }

        return "N/A|BadType";
    }

    private void ChatDevLogging(XivChatType type, int timestamp, SeString sender, SeString message, bool isHandled, string parsedSenderName)
    {
        if (parsedSenderName == "N/A|BadType")
        {
            Logger.Error("NEW CHAT MESSAGE: UNABLE TO PARSE NAME");
            Logger.Error("=======================================================");
            Logger.Error("Message Type: " + type.ToString());
            Logger.Error("Is Marked Handled: " + isHandled.ToString());
            Logger.Error("Raw Sender: " + sender.TextValue);
            Logger.Error("Parsed Sender: " + parsedSenderName);
        }
        else
        {
            Logger.Debug("NEW CHAT MESSAGE RECEIVED");
            Logger.Debug("=======================================================");
            Logger.Debug("Message Type: " + type.ToString());
            Logger.Debug("Is Marked Handled: " + isHandled.ToString());
            Logger.Debug("Raw Sender: " + sender.TextValue);
            Logger.Debug("Parsed Sender: " + parsedSenderName);
        }


        if (PluginInterface.IsDev && advancedDebugLogger != null)
        {
            SeString modifiedSender = sender;

            advancedDebugLogger.AddNewMessage(new () {
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
