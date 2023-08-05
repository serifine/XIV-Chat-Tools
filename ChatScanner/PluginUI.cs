using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using ChatScanner.Models;
using Dalamud.Game.Text;

namespace ChatScanner
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration Configuration { get; set; }
        private PluginState PluginState { get; set; }

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }


        private bool autoScrollToBottom = false;
        public bool AutoScrollToBottom
        {
            get { return this.autoScrollToBottom; }
            set { this.autoScrollToBottom = value; }
        }

        private string comboCurrentValue = "Focus Target";

        private readonly Vector4 ORANGE_COLOR = new Vector4(0.950f, 0.500f, 0f, 1f);
        private readonly Vector4 LIGHT_ORANGE_COLOR = new Vector4(0.950f, 0.650f, 0f, 1f);

        private FocusTab customWindowFocusTab = new FocusTab("Private Window")
        {
            focusTargets = new List<string>()
        };

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration, PluginState state)
        {
            this.Configuration = configuration;
            this.PluginState = state;

            UpdateChannelsToLog();
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ORANGE_COLOR);
            ImGui.PushStyleColor(ImGuiCol.CheckMark, ORANGE_COLOR);

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));

            var scale = ImGui.GetIO().FontGlobalScale;

            if (ImGui.Begin("Chat Scanner", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Checkbox("Auto scroll on new messages.", ref autoScrollToBottom);
                ImGui.SameLine(ImGui.GetContentRegionAvail().X - (100 * scale));
                if (ImGui.Button("Add Focus Target"))
                {
                    PluginState.AddFocusTabFromTarget();
                }

                // ImGui.Separator();

                if (ImGui.BeginTabBar("MainTabs", ImGuiTabBarFlags.Reorderable))
                {
                    if (ImGui.BeginTabItem("Dev Tab 2"))
                    {
                        SelectedTargetTab();

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Selected Target"))
                    {
                        SelectedTargetTab();

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("All Messages"))
                    {
                        AllMessagesTab();

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Custom Watch"))
                    {
                        DrawCustomTab("DefaultCustom");

                        ImGui.EndTabItem();
                    }

                    foreach (var focusTab in PluginState.GetFocusTabs())
                    {
                        if (ImGui.BeginTabItem(focusTab.Name, ref focusTab.Open, ImGuiTabItemFlags.None))
                        {
                            DrawFocusTab(focusTab.FocusTabId.ToString(), focusTab);

                            ImGui.EndTabItem();
                        }
                    }

                    PluginState.RemoveClosedFocusTabs();

                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }


        public void SelectedTargetTab()
        {
            var focusTarget = PluginState.GetCurrentOrMouseoverTarget();

            if (focusTarget != null)
            {
                var messages = PluginState.GetMessagesForFocusTarget();

                if (messages != null && messages.Count() > 0)
                {
                    MessagePanel(messages);
                }
                else
                {
                    ImGui.Text("No messages found for " + focusTarget.Name + ".");
                }
            }
            else
            {
                ImGui.Text("No target selected.");
            }
        }

        public void AllMessagesTab()
        {

            var tabMessages = PluginState.GetAllMessages();

            if (tabMessages.Count() > 0)
            {
                MessagePanel(tabMessages);
            }
            else
            {
                ImGui.Text("No messages to display.");
            }
        }

        public void DrawCustomTab(string tabId)
        {
            var scale = ImGui.GetIO().FontGlobalScale;

            if (ImGui.BeginTable("table1", 2, ImGuiTableFlags.NoHostExtendX))
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                foreach (var name in customWindowFocusTab.GetFocusTargets())
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    if (ImGui.SmallButton("Remove##" + tabId + name))
                    {
                        customWindowFocusTab.RemoveFocusTarget(name);
                    }

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(name);
                }
                ImGui.EndTable();
            }


            ImGui.SameLine(ImGui.GetContentRegionAvail().X - (300 * scale));
            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo(" ", comboCurrentValue))
            {
                if (ImGui.Selectable("Focus Target"))
                {
                    comboCurrentValue = "Focus Target";
                }

                if (ImGui.Selectable(PluginState.GetPlayerName() + " (you)"))
                {
                    // focusTab.AddFocusTarget(StateRepository.GetPlayerName());
                    comboCurrentValue = PluginState.GetPlayerName();
                }

                ImGui.Separator();

                foreach (var actor in PluginState.GetNearbyPlayers())
                {
                    if (ImGui.Selectable(actor.Name.TextValue))
                    {
                        comboCurrentValue = actor.Name.TextValue;
                    }
                }

                ImGui.EndCombo();
            }
            ImGui.SameLine();
            if (ImGui.Button("Add To Group"))
            {
                if (comboCurrentValue == "Focus Target")
                {
                    var focusTarget = PluginState.GetCurrentOrMouseoverTarget();

                    if (focusTarget != null)
                    {
                        customWindowFocusTab.AddFocusTarget(focusTarget.Name.TextValue);
                    }
                }
                else
                {
                    customWindowFocusTab.AddFocusTarget(comboCurrentValue);
                }

                comboCurrentValue = "Focus Target";
            }

            ImGui.Separator();

            var tabMessages = PluginState.GetMessagesByPlayerNames(customWindowFocusTab.GetFocusTargets());

            if (tabMessages.Count() > 0)
            {
                MessagePanel(tabMessages);
            }
            else
            {
                ImGui.Text("No messages to display.");
            }
        }

        public void DrawFocusTab(string tabId, FocusTab focusTab)
        {

            var tabMessages = PluginState.GetMessagesByPlayerNames(focusTab.GetFocusTargets());

            if (tabMessages.Count() > 0)
            {
                MessagePanel(tabMessages);
            }
            else
            {
                ImGui.Text("No messages to display.");
            }
        }

        public Vector4 color;

        public void MessagePanel(List<ChatEntry> messages)
        {
            ImGui.BeginChild("Messages");

            ImGui.ColorEdit4("", ref color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs);


            var isChatAtBottom = ImGui.GetScrollY() == ImGui.GetScrollMaxY();

            foreach (var chatItem in messages)
            {
                if (chatItem.SenderName == PluginState.GetPlayerName())
                {
                    ImGui.TextColored(ORANGE_COLOR, chatItem.DateSent.ToShortTimeString() + " " + chatItem.SenderName + ": ");
                    ImGui.SameLine();
                    ImGui.TextWrapped(chatItem.Message);
                }
                else
                {
                    ImGui.Spacing();
                    ImGui.Text(chatItem.DateSent.ToShortTimeString() + " " + chatItem.SenderName + ": ");
                    ImGui.SameLine();
                    ImGui.TextWrapped(chatItem.Message);
                }
            }

            if (AutoScrollToBottom == true)
            {
                ImGui.SetScrollY(ImGui.GetScrollMaxY());
            }

            ImGui.EndChild();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }


            var scale = ImGui.GetIO().FontGlobalScale;

            ImGui.SetNextWindowSize(new Vector2(400, 350), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(400, 350), new Vector2(float.MaxValue, float.MaxValue));

            if (ImGui.Begin("Chat Scanner Configuration", ref this.settingsVisible, ImGuiWindowFlags.None))
            {
                RenderStandardSettings();

                RenderChannelLoggingSettings();

                RenderMessagePersistenceOptions();

                RenderDevLogging();
            }
            ImGui.End();
        }

        private void RenderStandardSettings()
        {
            if (ImGui.Checkbox("Open On Login", ref this.Configuration.OpenOnLogin))
            {
                this.Configuration.Save();
            }

            if (ImGui.Checkbox("Preserve Message History on Logout", ref this.Configuration.MessageLog_PreserveOnLogout))
            {
                this.Configuration.Save();
            }
        }

        private int ChannelLogging_ActiveSelection = 0;
        private int ChannelLogging_InactiveSelection = 0;

        private String[] InactiveChannels;
        private String[] ActiveChannels;

        private void AddActiveChannel()
        {
            var channel = Configuration.AllChannels.First(t => t.Name == InactiveChannels[ChannelLogging_InactiveSelection]);

            Configuration.ActiveChannels.Add(channel.ChatType);

            UpdateChannelsToLog();
            Configuration.Save();
        }

        private void RemoveActiveChannel()
        {
            var channel = Configuration.AllChannels.First(t => t.Name == ActiveChannels[ChannelLogging_ActiveSelection]);

            Configuration.ActiveChannels.Remove(channel.ChatType);

            UpdateChannelsToLog();
            Configuration.Save();
        }

        private void UpdateChannelsToLog()
        {
            InactiveChannels = Configuration.AllChannels
              .Where(t => Configuration.ActiveChannels.Contains(t.ChatType) == false)
              .Select(t => t.Name)
              .OrderBy(t => t)
              .ToArray();
            ActiveChannels = Configuration.AllChannels
              .Where(t => Configuration.ActiveChannels.Contains(t.ChatType) == true)
              .Select(t => t.Name)
              .OrderBy(t => t)
              .ToArray();
        }

        private void RenderChannelLoggingSettings()
        {
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Text("Channels to Log");
            ImGui.Spacing();
            ImGui.Spacing();



            ImGui.SetNextItemWidth(260);
            ImGui.PushID("AddChannelComboBox");
            ImGui.Combo("", ref ChannelLogging_InactiveSelection, InactiveChannels, InactiveChannels.Count());
            ImGui.PopID();
            ImGui.SameLine();
            if (ImGui.Button("Add Selected Channel")) AddActiveChannel();

            // ImGui.PushItemWidth(180);
            ImGui.PushID("InactiveChannelsListbox");
            ImGui.SetNextItemWidth(400);
            ImGui.ListBox("", ref ChannelLogging_ActiveSelection, ActiveChannels, ActiveChannels.Count());
            ImGui.PopID();
            if (ImGui.Button("Remove Selected Channel From Watch List")) RemoveActiveChannel();

            // ImGui.SameLine();

            // ImGui.PushID("ActiveChannelsListbox");
            // ImGui.ListBox("", ref currentActiveSelection, ActiveChannels, ActiveChannels.Count());
            // ImGui.PopID();
            // ImGui.PopItemWidth();
        }

        private void RenderMessagePersistenceOptions()
        {
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Text("Message Saving");
            ImGui.Spacing();
            ImGui.Spacing();

            if (ImGui.Checkbox("Preserve Messages on Logout", ref this.Configuration.MessageLog_PreserveOnLogout))
            {
                this.Configuration.Save();
            }

            if (ImGui.Checkbox("Delete Old Messages", ref this.Configuration.MessageLog_DeleteOldMessages))
            {
                this.Configuration.Save();
            }

            if (this.Configuration.MessageLog_DeleteOldMessages)
            {

                if (ImGui.InputInt("Delete After (days)", ref this.Configuration.MessageLog_DaysToKeepOldMessages))
                {
                    this.Configuration.Save();
                }
            }

            ImGui.InputText("Message-Log File Path", ref this.Configuration.MessageLog_FilePath, 2048);
        }

        private void RenderDevLogging()
        {
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Text("Dev Logging");
            ImGui.Spacing();
            ImGui.Spacing();

            if (ImGui.Checkbox("Enable Debug Logging", ref this.Configuration.DebugLogging))
            {
                this.Configuration.Save();
            }

            if (Configuration.DebugLogging)
            {

                if (ImGui.Checkbox("Track target tab creation", ref this.Configuration.DebugLoggingCreatingTab))
                {
                    this.Configuration.Save();
                }
                // if (ImGui.Checkbox("Track target changing", ref this.configuration.DebugLoggingTargetChanging))
                // {
                //   this.configuration.Save();
                // }
                if (ImGui.Checkbox("Log Messages", ref this.Configuration.DebugLoggingMessages))
                {
                    this.Configuration.Save();
                }

                if (Configuration.DebugLoggingMessages)
                {
                    ImGui.Text("--");
                    ImGui.SameLine();
                    if (ImGui.Checkbox("Log Message Payloads", ref this.Configuration.DebugLoggingMessagePayloads))
                    {
                        this.Configuration.Save();
                    }

                    ImGui.Text("--");
                    ImGui.SameLine();
                    if (ImGui.Checkbox("Log Message Contents", ref this.Configuration.DebugLoggingMessageContents))
                    {
                        this.Configuration.Save();
                    }

                    ImGui.Text("--");
                    ImGui.SameLine();
                    if (ImGui.Checkbox("Log Message As JSON", ref this.Configuration.DebugLoggingMessageAsJson))
                    {
                        this.Configuration.Save();
                    }
                }
            }
        }
    }
}
