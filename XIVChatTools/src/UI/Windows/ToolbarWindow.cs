

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
    
    private WindowManagerService WindowManager => _plugin.WindowManagerService;
    private TabControllerService TabController => _plugin.TabController;

    private Configuration Configuration => _plugin.Configuration;
    private float Scale => ImGui.GetIO().FontGlobalScale;

    internal ToolbarWindow(Plugin plugin) : base("Toolbar###ChatToolsToolbar")
    {
        _plugin = plugin;

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
        ImGui.SetNextWindowSize(new Vector2(320 * Scale, 200 * Scale));
        if (ImGui.BeginPopup("Alerts"))
        {
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.TextWrapped("You can set up watchers that will make a notification sound whenever you receive a message that contains the selected phrase.");
            ImGui.Spacing();
            ImGui.TextWrapped("These phrases need to be separated by a comma.");
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Text("Global Watchers");

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint("", "Example, watch example", ref Configuration.MessageLog_Watchers, 24096))
            {
                Configuration.Save();
            }

            ImGui.EndPopup();
        }
    }

    private void DrawInterface()
    {
        if (ImGui.Button("New Watch Tab")) {
            var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();
            
            if (focusTarget != null)
            {
                TabController.AddFocusTab(focusTarget);
                WindowManager.MainWindow.IsOpen = true;
            } 
        }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add a new tab watching your selected target.");

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - (117 * Scale));

        if (ImGuiComponents.IconButton(WindowManager.MainWindow.IsOpen ? FontAwesomeIcon.EyeSlash : FontAwesomeIcon.Eye))
            WindowManager.MainWindow.Toggle();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(WindowManager.MainWindow.IsOpen ? "Hide Main Window" : "Show Main Window");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Search))
            WindowManager.SearchWindow.Toggle();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Search Messages");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Bell))
            ImGui.OpenPopup("Alerts");

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Chat Alerts");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
            WindowManager.SettingsWindow.Toggle();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Settings");
    }

    public override void Draw()
    {
        DrawPopups();
        DrawInterface();
    }
}
