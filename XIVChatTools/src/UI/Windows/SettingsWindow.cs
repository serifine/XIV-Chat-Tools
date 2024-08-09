

using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XIVChatTools.Services;

namespace XIVChatTools.UI.Windows;

public class SettingsWindow : Window
{
    private readonly Plugin _plugin;

    private Configuration Configuration => _plugin.Configuration;

    private int ChannelLogging_ActiveSelection = 0;
    private int ChannelLogging_InactiveSelection = 0;
    private String[] InactiveChannels = [];
    private String[] ActiveChannels = [];

    internal SettingsWindow(Plugin plugin, WindowManagerService windowManagerService) : base($"Chat Tools Settings###ChatToolsSettingsWindow")
    {
        _plugin = plugin;

        Size = new Vector2(400, 350);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoDocking;
    }


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
        ImGui.Combo("", ref ChannelLogging_InactiveSelection, InactiveChannels, InactiveChannels.Length);
        ImGui.PopID();
        ImGui.SameLine();
        if (ImGui.Button("Add Selected Channel")) AddActiveChannel();

        ImGui.PushItemWidth(180);
        ImGui.PushID("InactiveChannelsListbox");
        ImGui.SetNextItemWidth(400);
        ImGui.ListBox("", ref ChannelLogging_ActiveSelection, ActiveChannels, ActiveChannels.Length);
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


    public override void Draw()
    {
        DrawStandardSettings();
        DrawChannelLoggingSettings();
        DrawMessagePersistenceOptions();
        DrawDevLogging();
    }
}
