

using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace XIVChatTools.UI.Windows;

public class SettingsWindow : Window
{
    private readonly Plugin _plugin;

    private Configuration Configuration => _plugin.Configuration;

    private int _channelLoggingActiveSelection = 0;
    private int _channelLoggingInactiveSelection = 0;
    private String[] _inactiveChannels = [];
    private String[] _activeChannels = [];


    internal SettingsWindow(Plugin plugin) : base($"Chat Tools Settings###ChatToolsSettingsWindow")
    {
        _plugin = plugin;

        Size = new Vector2(400, 350);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoDocking;

        UpdateChannelsToLog();
    }


    private void AddActiveChannel()
    {
        var channel = Constants.ChatTypes.SupportedChannels.FirstOrDefault(t => t.Name == _inactiveChannels[_channelLoggingInactiveSelection]);


        if (channel != null)
        {
            Configuration.ActiveChannels.Add(channel.ChatType);

            UpdateChannelsToLog();
            Configuration.Save();
        }
    }

    private void RemoveActiveChannel()
    {
        var channel = Constants.ChatTypes.SupportedChannels.FirstOrDefault(t => t.Name == _activeChannels[_channelLoggingActiveSelection]);

        if (channel != null)
        {
            Configuration.ActiveChannels.Remove(channel.ChatType);

            UpdateChannelsToLog();
            Configuration.Save();
        }
    }

    private void UpdateChannelsToLog()
    {
        _inactiveChannels = Constants.ChatTypes.SupportedChannels
          .Where(t => Configuration.ActiveChannels.Contains(t.ChatType) == false)
          .Select(t => t.Name)
          .OrderBy(t => t)
          .ToArray();
        _activeChannels = Constants.ChatTypes.SupportedChannels
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

        if (ImGui.Checkbox("Preserve Message History on Logout", ref this.Configuration.MessageLogPreserveOnLogout))
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

        // if (ImGui.InputText("", ref Configuration.MessageLog_Watchers, 24096))
        // {
        //     Configuration.Save();
        // }

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
        ImGui.Combo("", ref _channelLoggingInactiveSelection, _inactiveChannels, _inactiveChannels.Length);
        ImGui.PopID();
        ImGui.SameLine();
        if (ImGui.Button("Add Selected Channel")) AddActiveChannel();

        ImGui.PushItemWidth(180);
        ImGui.PushID("InactiveChannelsListbox");
        ImGui.SetNextItemWidth(400);
        ImGui.ListBox("", ref _channelLoggingActiveSelection, _activeChannels, _activeChannels.Length);
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

        if (ImGui.Checkbox("Preserve Messages on Logout", ref this.Configuration.MessageLogPreserveOnLogout))
        {
            this.Configuration.Save();
        }

        if (ImGui.Checkbox("Delete Old Messages", ref this.Configuration.MessageLogDeleteOldMessages))
        {
            this.Configuration.Save();
        }

        if (this.Configuration.MessageLogDeleteOldMessages)
        {

            if (ImGui.InputInt("Delete After (days)", ref this.Configuration.MessageLogDaysToKeepOldMessages))
            {
                this.Configuration.Save();
            }
        }
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
