

using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XIVChatTools.Services;

namespace XIVChatTools.UI.Windows;

public class ToolbarWindow : Window
{
    private readonly Plugin _plugin;
    private readonly WindowManagerService _windowManagerService;

    private Configuration Configuration => _plugin.Configuration;
    private float Scale => ImGui.GetIO().FontGlobalScale;

    internal ToolbarWindow(Plugin plugin, WindowManagerService windowManagerService) : base($"Toolbar###{Plugin.Name}")
    {
        _plugin = plugin;
        _windowManagerService = windowManagerService;

        Size = new Vector2(450, 50);
        Flags = ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoScrollWithMouse
            | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize;
    }

    private void DrawPopups() {
        if (ImGui.BeginPopup("Alerts"))
        {
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Text("You can set up watchers that will make a notification sound whenever you receive a message that containers the selected phrase.");
            ImGui.Spacing();
            ImGui.Text("These phrases need to be separated by a comma.");
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Text("Global Watchers");

            if (ImGui.InputTextWithHint("", "Example, watch example", ref Configuration.MessageLog_Watchers, 24096))
            {
                Configuration.Save();
            }

            ImGui.EndPopup();
        }
    }

    private void DrawInterface() {
        if (ImGui.Button("Ctools Window"))
        {
            _windowManagerService.PluginUI.visible = !_windowManagerService.PluginUI.visible;
        }

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - (80 * Scale));

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Search))
            // searchWindowVisible = !searchWindowVisible;

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Search Messages");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Bell))
            ImGui.OpenPopup("Alerts");

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Chat Alerts");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
            // settingsVisible = !settingsVisible;

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Settings");
    }


    public override void Draw()
    {
        DrawPopups();
        DrawInterface();
    }
}
