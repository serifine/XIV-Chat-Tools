﻿using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using XIVChatTools.Models;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Components;
using XIVChatTools.Services;

namespace XIVChatTools;

class PluginUI : IDisposable
{
    public bool visible = false;
    public bool searchWindowVisible = false;
    public bool settingsVisible = false;
    private bool autoScrollToBottom = false;

    private Plugin _plugin { get; set; }
    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;

    private float Scale
    {
        get { return ImGui.GetIO().FontGlobalScale; }
    }

    private string comboCurrentValue = "Focus Target";
    private string searchText = "";
    List<ChatEntry> searchMessages = [];

    private FocusTab customWindowFocusTab = new FocusTab("Private Window")
    {
        focusTargets = new List<string>()
    };

    // passing in the image here just for simplicity
    public PluginUI(Plugin plugin)
    {
        _plugin = plugin;

        var io = ImGui.GetIO();

        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        UpdateChannelsToLog();
    }

    public void Dispose()
    {
    }

    public void Draw()
    {
        DrawMainWindow();
        DrawSearchWindow();
        DrawSettingsWindow();
    }

    public void DrawSearchWindow()
    {
        if (!searchWindowVisible)
        {
            return;
        }

        ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));

        if (ImGui.Begin("Search Messages", ref searchWindowVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDocking))
        {
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint("", "Search Messages", ref searchText, 24096))
            {
                searchMessages = PluginState.SearchMessages(searchText);
            }

            ImGui.Separator();

            if (searchMessages.Count > 0)
            {
                DrawMessagePanel(searchMessages);
            }
            else
            {
                ImGui.Text("No messages to display.");
            }

            ImGui.End();
        }

    }

    public void DrawMainWindow()
    {
        if (!visible)
        {
            return;
        }

        ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        if (ImGui.Begin("Chat Tools", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoBackground))
        {
            bool isWindowFocused = ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows | ImGuiFocusedFlags.RootWindow | ImGuiFocusedFlags.DockHierarchy);

            unsafe
            {
                Vector4 bgColor;

                if (isWindowFocused)
                {
                    bgColor = *ImGui.GetStyleColorVec4(ImGuiCol.TitleBgActive);
                }
                else
                {
                    bgColor = *ImGui.GetStyleColorVec4(ImGuiCol.TitleBg);
                }

                ImGui.PushStyleColor(ImGuiCol.ChildBg, bgColor);
            }

            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

            if (ImGui.BeginChild("ChildTest", new Vector2(0, 30), false))
            {
                ImGui.SameLine(8);
                ImGui.SameLine(ImGui.GetContentRegionAvail().X - (120 * Scale));
                if (ImGui.Button("Add Focus Target"))
                {
                    PluginState.AddFocusTabFromTarget();
                }
                ImGui.EndChild();
            }

            ImGui.PopStyleVar();
            ImGui.PopStyleColor();

            uint dockspaceId = ImGui.GetID("ChatToolsDockspace");
            ImGui.DockSpace(dockspaceId);


            // add padding for all child windows
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 8));

            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.Appearing);
            if (ImGui.Begin("Selected Target"))
            {
                DrawSelectedTargetTab();
                ImGui.End();
            }

            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.Appearing);
            if (ImGui.Begin("Custom Watch"))
            {
                DrawCustomTab("DefaultCustom");
                ImGui.End();
            }

            foreach (var focusTab in PluginState.GetFocusTabs())
            {
                ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.Appearing);
                if (ImGui.Begin(focusTab.Name, ref focusTab.Open))
                {
                    DrawFocusTab(focusTab.FocusTabId.ToString(), focusTab);

                    ImGui.End();
                }
            }

            // remove child window padding style
            ImGui.PopStyleVar();
            ImGui.PopStyleVar();

            PluginState.RemoveClosedFocusTabs();

            ImGui.End();
        }

        ImGui.PopStyleVar();
    }


    public void DrawSelectedTargetTab()
    {
        var focusTarget = PluginState.GetCurrentOrMouseoverTarget();

        if (focusTarget != null)
        {
            var messages = PluginState.GetMessagesForFocusTarget();

            if (messages != null && messages.Count() > 0)
            {
                DrawMessagePanel(messages);
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

    public void DrawCustomTab(string tabId)
    {
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


        ImGui.SameLine(ImGui.GetContentRegionAvail().X - (300 * Scale));
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
            DrawMessagePanel(tabMessages);
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
            DrawMessagePanel(tabMessages);
        }
        else
        {
            ImGui.Text("No messages to display.");
        }
    }

    public void DrawMessagePanel(List<ChatEntry> messages)
    {
        ImGui.BeginChild("Messages");

        var isChatAtBottom = ImGui.GetScrollY() == ImGui.GetScrollMaxY();

        if (ImGui.BeginTable("table1", 2, ImGuiTableFlags.NoHostExtendX))
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            foreach (var chatEntry in messages)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                SetNameColor(chatEntry);

                if (Configuration.SplitDateAndNames == true)
                {
                    ImGui.Text(chatEntry.SenderName);
                    ImGui.Text(chatEntry.DateSent.ToShortTimeString());
                }
                else
                {
                    ImGui.Text(chatEntry.DateSent.ToShortTimeString() + " " + chatEntry.SenderName + ": ");
                }
                ImGui.PopStyleColor();

                ImGui.TableSetColumnIndex(1);

                SetMessageColor(chatEntry);
                ImGuiHelpers.SafeTextWrapped(chatEntry.Message);
                ImGui.PopStyleColor();
            }


            ImGui.EndTable();
        }

        if (isChatAtBottom == true)
        {
            ImGui.SetScrollHereY(1.0f);
        }

        ImGui.EndChild();
    }

    private void SetNameColor(ChatEntry chatEntry)
    {
        if (chatEntry.SenderName == PluginState.GetPlayerName())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.CharacterNameColor);
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.NormalChatColor);
        }
    }

    private void SetMessageColor(ChatEntry chatEntry)
    {
        if (Configuration.DisableCustomChatColors)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.NormalChatColor);
        }
        else if (chatEntry.ChatType == XivChatType.CustomEmote || chatEntry.ChatType == XivChatType.StandardEmote)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.EmoteColor);
        }
        else if (chatEntry.ChatType == XivChatType.TellIncoming || chatEntry.ChatType == XivChatType.TellOutgoing)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.TellColor);
        }
        else if (chatEntry.ChatType == XivChatType.Party)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.PartyColor);
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.NormalChatColor);
        }
    }


    #region Settings UI

    public void DrawSettingsWindow()
    {
        if (!settingsVisible)
        {
            return;
        }

        ImGui.SetNextWindowSize(new Vector2(400, 350), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(400, 350), new Vector2(float.MaxValue, float.MaxValue));

        if (ImGui.Begin("Chat Tools Configuration", ref this.settingsVisible, ImGuiWindowFlags.NoDocking))
        {
            DrawStandardSettings();

            DrawChannelLoggingSettings();

            DrawMessagePersistenceOptions();

            DrawDevLogging();
        }

        ImGui.End();
    }

    private void DrawStandardSettings()
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

    private String[] InactiveChannels = [];
    private String[] ActiveChannels = [];

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

    private void DrawChannelLoggingSettings()
    {
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Notify when message contains (comma delimited)");
        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.InputText("", ref Configuration.MessageLog_Watchers, 24096))
        {
            Configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.Checkbox("Split date and names on new lines", ref Configuration.SplitDateAndNames))
        {
            Configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Chat Colors");
        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.Checkbox("Disable Custom Chat Colors", ref Configuration.DisableCustomChatColors))
        {
            Configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.ColorEdit4("My Name Color", ref Configuration.CharacterNameColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
        {
            Configuration.Save();
        }
        if (ImGui.ColorEdit4("Normal Message Color", ref Configuration.NormalChatColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
        {
            Configuration.Save();
        }
        if (ImGui.ColorEdit4("Emote Color", ref Configuration.EmoteColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
        {
            Configuration.Save();
        }
        if (ImGui.ColorEdit4("Tell Color", ref Configuration.TellColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
        {
            Configuration.Save();
        }
        if (ImGui.ColorEdit4("Party Chat Color", ref Configuration.PartyColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
        {
            Configuration.Save();
        }

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

        ImGui.PushItemWidth(180);
        ImGui.PushID("InactiveChannelsListbox");
        ImGui.SetNextItemWidth(400);
        ImGui.ListBox("", ref ChannelLogging_ActiveSelection, ActiveChannels, ActiveChannels.Count());
        ImGui.PopID();
        if (ImGui.Button("Remove Selected Channel From Watch List")) RemoveActiveChannel();
    }

    private void DrawMessagePersistenceOptions()
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

    private void DrawDevLogging()
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
    }

    #endregion
}
