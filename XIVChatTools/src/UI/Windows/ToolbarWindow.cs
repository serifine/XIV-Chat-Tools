

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

public class ToolbarWindow : Window
{
    private readonly Plugin _plugin;
    private readonly WindowManagerService _windowManagerService;

    private Configuration Configuration => _plugin.Configuration;
    private float Scale => ImGui.GetIO().FontGlobalScale;

    private int selectedWindowType = 0;
    private string[] windowTypes = new string[] { "Watch Target", "Custom Window" };

    internal ToolbarWindow(Plugin plugin, WindowManagerService windowManagerService) : base("Toolbar###ChatToolsToolbar")
    {
        _plugin = plugin;
        _windowManagerService = windowManagerService;

        Size = new Vector2(450 * Scale, 40 * Scale);
        SizeCondition = ImGuiCond.Always;
        Flags = ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoScrollWithMouse
            | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize;
    }

    private void DrawPopups()
    {
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

    private void DrawInterface()
    {
        // if (ImGui.Button("Open Watch Window"))
        // {
        //     _windowManagerService.ChatToolsWindow.IsOpen = !_windowManagerService.ChatToolsWindow.IsOpen;
        // }

        // ImGui.SameLine();

        ImGui.SetNextItemWidth(140 * Scale);
        if (ImGui.Combo("", ref selectedWindowType, windowTypes, windowTypes.Length))
        {
            // on change
        }
        
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.PlusCircle)) {
            // on click
        }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add Tab");

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - (117 * Scale));

        if (ImGuiComponents.IconButton(_windowManagerService.MainWindow.IsOpen ? FontAwesomeIcon.EyeSlash : FontAwesomeIcon.Eye))
            _windowManagerService.MainWindow.Toggle();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(_windowManagerService.MainWindow.IsOpen ? "Hide Main Window" : "Show Main Window");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Search))
            _windowManagerService.SearchWindow.Toggle();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Search Messages");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Bell))
            ImGui.OpenPopup("Alerts");

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Chat Alerts");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
            _windowManagerService.SettingsWindow.Toggle();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Settings");
    }

    public override void Draw()
    {
        DrawPopups();
        DrawInterface();
    }
}
