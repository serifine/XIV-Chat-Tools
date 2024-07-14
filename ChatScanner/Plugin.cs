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
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Plugin.Services;

namespace ChatScanner
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Chat Scanner";

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IPluginLog Logger { get; private set; } = null!;

        internal PluginState PluginState { get; init; }
        internal Configuration Configuration { get; }
        internal PluginUI PluginUI { get; }

        private List<string> commandAliases = new List<string>() {
          "/chatScanner",
          "/cScanner",
          "/cscan"
        };

        private List<string> settingsArgumentAliases = new List<string>() {
          "settings",
          "config",
          "c"
        };

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);
            PluginState = PluginInterface.Create<PluginState>(Configuration);
            PluginUI = new PluginUI(Configuration, PluginState);
            PluginUI.Visible = Configuration.OpenOnLogin;

            foreach (string commandAlias in commandAliases)
            {
                CommandManager.AddHandler(commandAlias, new CommandInfo(OnCommand)
                {
                    HelpMessage = commandAliases.First() == commandAlias ?
                      "Opens the Chat Scanner window." : "Alias for /chatScanner."
                });
            }

            ChatGui.ChatMessage += Chat_OnChatMessage;

            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;

            PluginInterface.UiBuilder.OpenMainUi += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            PluginUI.Dispose();

            ChatGui.ChatMessage -= Chat_OnChatMessage;

            ClientState.Login -= OnLogin;
            ClientState.Logout -= OnLogout;

            PluginInterface.UiBuilder.OpenMainUi -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

            foreach (string commandAlias in commandAliases)
            {
                CommandManager.RemoveHandler(commandAlias);
            }
        }

        private void OnCommand(string command, string args)
        {
            if (settingsArgumentAliases.Contains(args.ToLower()))
            {
                PluginUI.SettingsVisible = true;
            }
            else
            {
                PluginUI.Visible = true;
            }
        }

        private void DrawUI()
        {
            PluginUI.Draw();
        }

        private void DrawConfigUI()
        {
            PluginUI.SettingsVisible = true;
        }

        private void OnLogin()
        {
            PluginUI.Visible = Configuration.OpenOnLogin;
        }

        private void OnLogout()
        {
            PluginUI.Visible = false;
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

            if (Configuration.DebugLogging && Configuration.DebugLoggingMessages)
            {
                Logger.Debug("NEW CHAT MESSAGE RECEIVED");
                Logger.Debug("=======================================================");
                Logger.Debug("Message Type: " + type.ToString());
                Logger.Debug("Is Marked Handled: " + isHandled.ToString());
                Logger.Debug("Raw Sender: " + sender.TextValue);
                Logger.Debug("Parsed Sender: " + parsedSenderName);

                if (Configuration.DebugLoggingMessagePayloads && sender.Payloads.Any())
                {
                    Logger.Debug("");
                    Logger.Debug("SenderPayloads");
                    foreach (var payload in sender.Payloads)
                    {
                        Logger.Debug("Type: " + payload.Type.ToString());
                        Logger.Debug(payload.ToString());
                    }
                }

                if (Configuration.DebugLoggingMessageContents)
                {
                    Logger.Debug("");
                    Logger.Debug("Message: " + message.TextValue);
                }

                if (Configuration.DebugLoggingMessageAsJson)
                {
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
            }

            var watchers = Configuration.MessageLog_Watchers.Split(",");
            var messageText = message.TextValue;

            

            if (Configuration.MessageLog_Watchers.Trim() != "" && watchers.Any(t => messageText.ToLower().Contains(t.ToLower().Trim())))
            {
                UIModule.PlayChatSoundEffect(2);
            }

            PluginState.AddChatMessage(new Models.ChatEntry()
            {
                ChatType = type,
                Message = message.TextValue,
                Timestamp = timestamp,
                SenderName = parsedSenderName
            });
        }

        private string ParseSenderName(XivChatType type, SeString sender)
        {
            var playerPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

            if (playerPayload != null)
            {
                return (playerPayload as PlayerPayload).PlayerName;
            }

            if (type == XivChatType.CustomEmote || type == XivChatType.StandardEmote)
            {
                var textPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.RawText);

                if (textPayload != null)
                {
                    return (textPayload as TextPayload).Text;
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
}
