

using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Microsoft.Extensions.Logging;
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
        ImGui.SetNextWindowSize(new Vector2(320 * Scale, 0));
        if (ImGui.BeginPopup("Alerts"))
        {
            string globalWatchers = Configuration.Session_WatchData.GlobalWatchers;
            string characterWatchers = Configuration.Session_WatchData.CharacterWatchers;
            string sessionWatchers = Configuration.Session_WatchData.SessionWatchers;

            ImGui.TextWrapped("You can set up watchers that will make a notification sound whenever you receive a message that contains the selected phrase.");
            ImGui.Spacing();
            ImGui.TextWrapped("These phrases need to be separated by a comma. To update the watchers, press Enter after after typing in the new phrases.");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Text("Global Watchers");
            ImGuiComponents.HelpMarker("These watchers are always active on all characters.");

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint("###GlobalWatcherInput", "Example, watch example", ref globalWatchers, 24096, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                Configuration.UpdateGlobalWatchers(globalWatchers);
                ImGui.CloseCurrentPopup();
            }

            ImGui.Text("Character Watchers");
            ImGuiComponents.HelpMarker("These watchers are only active on the current character.");

            if (ImGui.InputTextWithHint("###CharacterWatcherInput", "Example, watch example", ref characterWatchers, 24096, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                Configuration.UpdateCharacterWatchers(characterWatchers);
                ImGui.CloseCurrentPopup();
            }

            ImGui.Text("Session Watchers");
            ImGuiComponents.HelpMarker("These watchers are only active until you log out.");

            if (ImGui.InputTextWithHint("###SessionWatcherInput", "Example, watch example", ref sessionWatchers, 24096, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                Configuration.UpdateSessionWatchers(sessionWatchers);
                ImGui.CloseCurrentPopup();
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
