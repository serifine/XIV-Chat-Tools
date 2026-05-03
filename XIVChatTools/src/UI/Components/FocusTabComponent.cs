using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using XIVChatTools.Models;
using XIVChatTools.Models.Tabs;
using XIVChatTools.Services;

namespace XIVChatTools.UI.Components;


internal class FocusTabComponent(Plugin plugin)
{
    private readonly MessagePanel _messagePanel = new(plugin);

    private TabControllerService TabController => plugin.TabController;

    private float Scale => ImGui.GetIO().FontGlobalScale;

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// MAYBE MAKE INTO HELPER FUNCTIONS
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// DEFINITELY RE ORDER
    private string GetFocusTargetDisplayName(PlayerIdentifier focusTarget, List<PlayerIdentifier> focusTargets)
    {
        string focusTargetName = focusTarget.Name;

        if (focusTargets.Count(t => t.Name == focusTargetName) > 1)
        {
            focusTargetName += $" ({focusTarget.World})";
        }

        return focusTargetName;
    }

    internal void Draw(FocusTab focusTab)
    {
        var open = true;

        if (ImGui.BeginTabItem($"{focusTab.Title}###{focusTab.TabId}", ref open))
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// REFACTOR INTO SUB FUNCTION
            if (ImGui.BeginPopupContextItem($"{focusTab.TabId}TabItemContextMenu"))
            {
                ImGui.Text("Tab Options");
                ImGui.Separator();
                ImGui.SetNextItemWidth(200);
                ImGui.InputText($"###{focusTab.TabId}TabNameInput", ref focusTab.Title, 64);

                ImGui.EndPopup();
            }

            DrawContent(focusTab);

            ImGui.EndTabItem();
        }

        if (!open)
        {
            focusTab.Close();
        }
    }

    internal void DrawContent(FocusTab focusTab)
    {
        DrawFocusTabHeader(focusTab);
        DrawFocusTabBody(focusTab);
    }

    private void DrawFocusTabHeader(FocusTab focusTab)
    {
        var focusTargets = focusTab.GetFocusTargets();

        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 2));

        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0, 0, 0, 0));
        if (ImGui.BeginChild("###focusMemberScrollbarContainer", new Vector2(ImGui.GetContentRegionAvail().X, 24), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            var offset = DrawScrollableTargetBar(focusTab);

            ImGui.SameLine(0, offset);
            DrawAddPlayerButton(focusTab);

            ImGui.EndChild();
        }

        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(4);

    }

    private int DrawScrollableTargetBar(FocusTab focusTab)
    {
        var focusTargets = focusTab.GetFocusTargets();
        float scrollXDelta = 0.0f;
        float scrollX = 0.0f;
        float scrollMaxX = 0.0f;
        int scrollButtonOffsets = 8;

        if (ImGui.BeginChild("###focusMemberScrollbar", new Vector2(ImGui.GetContentRegionAvail().X - 142, 24), false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            scrollX = ImGui.GetScrollX();
            scrollMaxX = ImGui.GetScrollMaxX();

            ImGui.SameLine(5);

            foreach (var focusTarget in focusTargets)
            {
                var focusTabName = GetFocusTargetDisplayName(focusTarget, focusTargets);

                ImGui.Text(focusTabName);
                if (ImGui.BeginPopupContextItem("###" + focusTabName + "ContextMenu"))
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

                ImGui.SameLine();
            }

            ImGui.EndChild();
        }

        if (scrollMaxX > 0)
        {
            ImGui.SameLine();
            ImGui.Button("<", new Vector2(20, 20));
            if (ImGui.IsItemActive())
            {
                scrollXDelta = -ImGui.GetIO().DeltaTime * 1000.0f;
            }

            ImGui.SameLine();
            ImGui.Button(">", new Vector2(20, 20));
            if (ImGui.IsItemActive())
            {
                scrollXDelta = +ImGui.GetIO().DeltaTime * 1000.0f;
            }
        }
        else
        {
            scrollButtonOffsets = 64;
        }

        if (scrollXDelta != 0.0f)
        {
            ImGui.BeginChild("###focusMemberScrollbar");
            ImGui.SetScrollX(ImGui.GetScrollX() + scrollXDelta);
            ImGui.EndChild();
        }

        return scrollButtonOffsets;
    }

    private void DrawAddPlayerButton(FocusTab focusTab)
    {
        if (ImGui.Button("Add Player"))
        {
            ImGui.OpenPopup("###AddFocusTargetPopup");
        }

        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 8);
        if (ImGui.BeginPopup("###AddFocusTargetPopup"))
        {
            if (ImGui.Selectable("Focus Target"))
            {
                var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

                if (focusTarget != null)
                {
                    focusTab.AddFocusTarget(focusTarget);
                }

                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Selectable(Helpers.PlayerCharacter.Name + " (you)"))
            {
                var focusTarget = Helpers.PlayerCharacter.GetPlayerIdentifier();

                if (focusTarget != null)
                {
                    focusTab.AddFocusTarget(focusTarget);
                }

                ImGui.CloseCurrentPopup();
            }

            var nearbyPlayers = Helpers.NearbyPlayers.GetNearbyPlayers();

            if (nearbyPlayers.Count > 0)
            {
                ImGui.Separator();

                ImGui.BeginChild("###NearbyPlayers", new Vector2(200, 200));

                foreach (var actor in nearbyPlayers)
                {
                    if (ImGui.Selectable(actor.Name.TextValue))
                    {
                        var focusTarget = new PlayerIdentifier(actor);
                        focusTab.AddFocusTarget(focusTarget);
                        ImGui.CloseCurrentPopup();
                    }
                }

                ImGui.EndChild();
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
