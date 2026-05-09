using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using ChatTools.Models;
using ChatTools.Models.Tabs;
using ChatTools.Services;
using Dalamud.Interface.Utility.Raii;
using ChatTools.Helpers;
using Dalamud.Game.Text.SeStringHandling;

namespace ChatTools.UI.Components;


internal class FocusTabComponent(Plugin plugin)
{
    private readonly MessagePanel _messagePanel = new(plugin);

    private TabControllerService TabController => plugin.TabController;

    private float Scale => ImGui.GetIO().FontGlobalScale;

    internal void Draw(FocusTab focusTab)
    {
        DrawFocusTabHeader(focusTab);
        DrawFocusTabBody(focusTab);
    }

    private void DrawFocusTabHeader(FocusTab focusTab)
    {
        var focusTargets = focusTab.GetFocusTargets();

        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        var header = ImGui.BeginChild("###focusMemberScrollbarContainer", new Vector2(ImGui.GetContentRegionAvail().X, 24), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysUseWindowPadding);
        ImGui.PopStyleVar(2);

        if (header)
        {
            var offset = DrawScrollableTargetBar(focusTab);

            ImGui.SameLine(0, offset);
            DrawAddPlayerButton(focusTab);

            ImGui.EndChild();
        }
    }

    private int DrawScrollableTargetBar(FocusTab focusTab)
    {
        var focusTargets = focusTab.GetFocusTargets();
        float scrollXDelta = 0.0f;
        float scrollX = 0.0f;
        float scrollMaxX = 0.0f;
        int scrollButtonOffsets = 8;

        if (ImGui.BeginChild("###focusMemberScrollbar", new Vector2(ImGui.GetContentRegionAvail().X - 151, 24), false, ImGuiWindowFlags.NoScrollbar))
        {
            scrollX = ImGui.GetScrollX();
            scrollMaxX = ImGui.GetScrollMaxX();

            // ImGui.SameLine(5);

            foreach (var focusTarget in focusTargets)
            {
                DrawTargetChip(focusTarget, focusTab);
                ImGui.SameLine();
            }

            ImGui.EndChild();
        }

        // Left and Right Scroll Buttons
        if (scrollMaxX > 0)
        {
            ImGui.SameLine();
            ImGui.Button("<", new Vector2(20, 24));
            if (ImGui.IsItemActive())
            {
                scrollXDelta = -ImGui.GetIO().DeltaTime * 1000.0f;
            }

            ImGui.SameLine();
            ImGui.Button(">", new Vector2(20, 24));
            if (ImGui.IsItemActive())
            {
                scrollXDelta = +ImGui.GetIO().DeltaTime * 1000.0f;
            }
        }
        else
        {
            scrollButtonOffsets = 64;
        }

        // Adding this as it's not very clear, but this portion is used to control
        // the scrolling of the focus target bar through the buttons.
        if (scrollXDelta != 0.0f)
        {
            ImGui.BeginChild("###focusMemberScrollbar");
            ImGui.SetScrollX(ImGui.GetScrollX() + scrollXDelta);
            ImGui.EndChild();
        }

        return scrollButtonOffsets;
    }

    private void DrawTargetChip(PlayerIdentifier focusTarget, FocusTab focusTab)
    {
        var sameWorld = focusTarget.World == PlayerCharacter.World;

        var spaceSize = ImGui.CalcTextSize(" ");
        var width = ImGui.CalcTextSize(focusTarget.Name).X + 8 * 2;

        if (!sameWorld)
        {
            width += ImGui.CalcTextSize(focusTarget.World).X + spaceSize.Y;
        }

        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(1, 1, 1, 0.1f));
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 4));
        using (ImRaii.Child("###" + focusTarget.Name + focusTarget.World + "ContextMenu", new Vector2(width, 24), false, ImGuiWindowFlags.AlwaysUseWindowPadding))
        {
            ImGui.Text(focusTarget.Name);

            if (!sameWorld)
            {
                ImGui.SameLine(0, 0);
                ImGuiHelpers.DrawIcon(BitmapFontIcon.CrossWorld);
                ImGui.SameLine(0, 0);
                ImGui.Text(focusTarget.World);
            }
        }
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();

        if (ImGui.BeginPopupContextItem("###" + focusTarget.Name + focusTarget.World + "ContextMenu"))
        {
            if (ImGui.MenuItem("Create Watch Tab From Player"))
            {
                TabController.AddFocusTab(focusTarget);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("Remove Player From Group"))
            {
                focusTab.RemoveFocusTarget(focusTarget);
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private void DrawAddPlayerButton(FocusTab focusTab)
    {
        if (ImGui.Button("Add Player", new Vector2(83, 24)))
        {
            ImGui.OpenPopup("###AddFocusTargetPopup");
        }


        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 8);
        if (ImGui.BeginPopup("###AddFocusTargetPopup"))
        {
            var targets = focusTab.GetFocusTargets();

            var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();
            if (focusTarget != null)
            {
                if (ImGui.Selectable("Focus Target"))
                {
                    focusTab.AddFocusTarget(focusTarget);
                    ImGui.CloseCurrentPopup();
                }
            }

            var player = Helpers.PlayerCharacter.GetPlayerIdentifier();
            if (player != null && !targets.Any(t => t.Matches(player)) && ImGui.Selectable(Helpers.PlayerCharacter.Name + " (you)"))
            {
                if (player != null)
                {
                    focusTab.AddFocusTarget(player);
                }

                ImGui.CloseCurrentPopup();
            }

            var partyPlayers = Plugin.PartyList
                .Select(p => new PlayerIdentifier(p))
                .Where(p => targets.All(t => !t.Matches(p)))
                .ToList();

            if (partyPlayers.Any())
            {
                ImGui.Separator();
                ImGuiHelpers.DrawLabel("Party Members", 0.8f);

                foreach (var partyMember in partyPlayers)
                {
                    if (ImGui.Selectable(partyMember.Name + " (" + partyMember.World + ")"))
                    {
                        focusTab.AddFocusTarget(partyMember);
                        ImGui.CloseCurrentPopup();
                    }
                }
            }

            var nearbyPlayers = Helpers.NearbyPlayers.GetNearbyPlayers()
                .Select(p => new PlayerIdentifier(p))
                .Where(p => !targets.Any(t => t.Matches(p)) && !partyPlayers.Any(pp => pp.Matches(p)));

            if (nearbyPlayers.Any())
            {
                ImGui.Separator();
                ImGuiHelpers.DrawLabel("Nearby Players", 0.8f);

                foreach (var nearbyPlayer in nearbyPlayers)
                {
                    if (ImGui.Selectable(nearbyPlayer.Name + " (" + nearbyPlayer.World + ")"))
                    {
                        focusTab.AddFocusTarget(nearbyPlayer);
                        ImGui.CloseCurrentPopup();
                    }
                }
            }

            if (player == null && focusTarget == null && !partyPlayers.Any() && !nearbyPlayers.Any())
            {
                ImGui.Text("No players to add.");
            }

            ImGui.EndPopup();
        }
        ImGui.PopStyleVar();
    }

    private void DrawFocusTabBody(FocusTab focusTab)
    {
        var focusTargets = focusTab.GetFocusTargets();

        if (focusTab.Messages.Count > 0)
        {
            _messagePanel.Draw(focusTab.Messages);
        }
        else
        {
            ImGui.Text("No messages to display.");
        }

    }
}
