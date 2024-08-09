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
using Newtonsoft.Json;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using XIVChatTools.Services;

namespace XIVChatTools;

public class Plugin : IDalamudPlugin
{
    public static string Name => "Chat Scanner";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    internal readonly PluginStateService PluginState;
    internal readonly WindowManagerService WindowManagerService;
    internal readonly Configuration Configuration;

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

            #region Register Services

            PluginState = PluginInterface.Create<PluginStateService>(this)!;
            WindowManagerService = PluginInterface.Create<WindowManagerService>(this)!;

            if (PluginState == null) throw new Exception("Fatal Error: Failed to create PluginStateService.");
            if (WindowManagerService == null) throw new Exception("Fatal Error: Failed to create WindowManagerService.");

            #endregion


            ChatGui.ChatMessage += Chat_OnChatMessage;

            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenMainUi += OpenMainUI;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUI;

            foreach (string commandAlias in commandAliases)
            {
                CommandManager.AddHandler(commandAlias, new CommandInfo(OnCommand)
                {
                    HelpMessage = commandAliases.First() == commandAlias ?
                      "Opens the Chat Scanner window." : "Alias for /chattools."
                });
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        ChatGui.ChatMessage -= Chat_OnChatMessage;

        ClientState.Login -= OnLogin;
        ClientState.Logout -= OnLogout;

        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUI;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUI;

        if (WindowManagerService != null) WindowManagerService.Dispose();

        foreach (string commandAlias in commandAliases)
        {
            if (CommandManager.Commands.Any(t => t.Key == commandAlias)) {
                CommandManager.RemoveHandler(commandAlias);
            }
        }
    }

    private void OnCommand(string command, string args)
    {
        if (settingsArgumentAliases.Contains(args.ToLower()))
        {
            WindowManagerService.ToolbarWindow.IsOpen = !WindowManagerService.ToolbarWindow.IsOpen;
        }
        else
        {
            OpenMainUI();
        }
    }

    private void DrawUI()
    {
        WindowManagerService.Draw();
    }

    private void OpenMainUI()
    {
        WindowManagerService.ToolbarWindow.IsOpen = true;
    }

    private void OpenConfigUI()
    {
        // PluginUI.settingsVisible = true;
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
            PluginState.ClearMessageHistory();
        }
    }

    private void Chat_OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (Enum.IsDefined(typeof(XivChatType), type) == false || Configuration.AllChannels.Any(t => t.ChatType == type) == false || isHandled || !Configuration.ActiveChannels.Any(t => t == type))
        {
            return;
        }


        var parsedSenderName = ParseSenderName(type, sender);

        if (Configuration.DebugLogging && parsedSenderName == "N/A|BadType")
        {
            Logger.Error("NEW CHAT MESSAGE: UNABLE TO PARSE NAME");
            Logger.Error("=======================================================");
            Logger.Error("Message Type: " + type.ToString());
            Logger.Error("Is Marked Handled: " + isHandled.ToString());
            Logger.Error("Raw Sender: " + sender.TextValue);
            Logger.Error("Parsed Sender: " + parsedSenderName);
            Logger.Error("CMessage Json: ");
            try
            {
                Logger.Error(JsonConvert.SerializeObject(message, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred during serialization.");
                Logger.Error(ex.Message);
            }

            Logger.Error("Sender Json: ");
            try
            {
                Logger.Error(JsonConvert.SerializeObject(sender, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred during serialization.");
                Logger.Error(ex.Message);
            }
        }

        if (Configuration.DebugLogging)
        {
            Logger.Debug("NEW CHAT MESSAGE RECEIVED");
            Logger.Debug("=======================================================");
            Logger.Debug("Message Type: " + type.ToString());
            Logger.Debug("Is Marked Handled: " + isHandled.ToString());
            Logger.Debug("Raw Sender: " + sender.TextValue);
            Logger.Debug("Parsed Sender: " + parsedSenderName);

            if (sender.Payloads.Any())
            {
                Logger.Debug("");
                Logger.Debug("SenderPayloads");
                foreach (var payload in sender.Payloads)
                {
                    Logger.Debug("Type: " + payload.Type.ToString());
                    Logger.Debug(payload.ToString() ?? "");
                }
            }

            Logger.Debug("");
            Logger.Debug("Message: " + message.TextValue);

            Logger.Debug("");
            Logger.Debug("CMessage Json: ");
            try
            {
                Logger.Debug(JsonConvert.SerializeObject(message, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.Debug("An error occurred during serialization.");
                Logger.Debug(ex.Message);
            }

            Logger.Debug("Sender Json: ");
            try
            {
                Logger.Debug(JsonConvert.SerializeObject(sender, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.Debug("An error occurred during serialization.");
                Logger.Debug(ex.Message);
            }
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

        PluginState.AddChatMessage(new Models.ChatEntry()
        {
            ChatType = type,
            OwnerId = PluginState.GetPlayerName(),
            Message = message.TextValue,
            Timestamp = timestamp,
            SenderName = parsedSenderName
        });
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
}
