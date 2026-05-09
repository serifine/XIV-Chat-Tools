using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;
using ChatTools;

namespace ChatTools.UI.Components;

internal class WatcherConfigurationComponent(Plugin plugin)
{
    private Configuration Configuration => plugin.Configuration;

    internal void Draw()
    {
        string globalWatchers = Configuration.SessionWatchData.GlobalWatchers;
        string characterWatchers = Configuration.SessionWatchData.CharacterWatchers;
        string sessionWatchers = Configuration.SessionWatchData.SessionWatchers;

        ImGui.TextWrapped("You can set up watchers that will make a notification sound whenever you receive a message that contains the selected phrase.");
        ImGui.Spacing();
        ImGui.TextWrapped("These phrases need to be separated by a comma. To update the watchers, press Enter after after typing in the new phrases.");
        ImGui.Spacing();
        ImGui.Spacing();
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
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);

        if (ImGui.InputTextWithHint("###CharacterWatcherInput", "Example, watch example", ref characterWatchers, 24096, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            Configuration.UpdateCharacterWatchers(characterWatchers);
            ImGui.CloseCurrentPopup();
        }

        ImGui.Text("Session Watchers");
        ImGuiComponents.HelpMarker("These watchers are only active until you log out.");
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputTextWithHint("###SessionWatcherInput", "Example, watch example", ref sessionWatchers, 24096, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            Configuration.UpdateSessionWatchers(sessionWatchers);
            ImGui.CloseCurrentPopup();
        }
    }
}
