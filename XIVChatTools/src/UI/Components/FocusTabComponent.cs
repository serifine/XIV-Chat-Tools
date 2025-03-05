

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XIVChatTools.Models;
using XIVChatTools.Models.Tabs;
using XIVChatTools.Services;

namespace XIVChatTools.UI.Components;


internal class FocusTabComponent
{
    private readonly Plugin _plugin;
    private readonly MessagePanel _messagePanel;

    private readonly float Item_Height = 24f;
    private readonly float Item_Width = 250f;

    private TabControllerService TabController => _plugin.TabController;

    private float Scale => ImGui.GetIO().FontGlobalScale;
    private PlayerIdentifier comboCurrentValue = new PlayerIdentifier("Focus Target", "");

    public FocusTabComponent(Plugin plugin)
    {
        _plugin = plugin;
        _messagePanel = new(plugin);
    }

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

        if (ImGui.BeginTabItem(focusTab.Title, ref open))
        {
            DrawFocusTabHeader(focusTab);
            DrawFocusTabBody(focusTab);

            ImGui.EndTabItem();
        }

        if (!open)
        {
            focusTab.Close();
        }
    }

    private void DrawFocusTabHeader(FocusTab focusTab)
    {
        var focusTargets = focusTab.GetFocusTargets();

        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 2));

        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(1, 1, 1, 0.0f));
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0, 0, 0, 0));
        if (ImGui.BeginChild("###focusMemberScrollbarContainer", new Vector2(ImGui.GetContentRegionAvail().X, 24), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            var offset = DrawScrollTabBar(focusTab);

            ImGui.SameLine(0, offset);
            DrawAddPlayerButton(focusTab);

            ImGui.EndChild();
        }

        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(4);

    }

    private int DrawScrollTabBar(FocusTab focusTab)
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
                        var focusTarget = new PlayerIdentifier(actor.Name.TextValue, actor.HomeWorld.Value.Name.ToString() ?? "Unknown World");
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

        if (focusTab.messages.Count > 0)
        {
            _messagePanel.Draw(focusTab.messages);
        }
        else
        {
            ImGui.Text("No messages to display.");
        }
        
        // if (ImGui.BeginTable("table1", 2, ImGuiTableFlags.NoHostExtendX))
        // {
        //     ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 4f));

        //     ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        //     ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

        //     ImGui.TableNextRow();
        //     ImGui.TableSetColumnIndex(0);

        //     if (focusTargets.Count > 1)
        //     {
        //         TargetComboBox();
        //         ImGui.SameLine();
        //         AddWatchTargetButton("Add", focusTab, 50);
        //         FocusWatchList(focusTab, focusTargets);
        //     }

        //     ImGui.TableSetColumnIndex(1);

        //     if (focusTab.messages.Count > 0)
        //     {
        //         _messagePanel.Draw(focusTab.messages);
        //     }
        //     else
        //     {
        //         ImGui.Text("No messages to display.");
        //     }

        //     ImGui.PopStyleVar();

        //     ImGui.EndTable();
        // }
    }

    private void FocusWatchList(FocusTab focusTab, List<PlayerIdentifier> focusTargets)
    {
        if (focusTargets.Count <= 1)
        {
            return;
        }

        float windowHeight = ImGui.GetContentRegionAvail().Y;

        if (ImGui.BeginListBox("##WatchTargetBox", new Vector2(Item_Width, windowHeight)))
        {
            foreach (var focusTarget in focusTab.GetFocusTargets())
            {
                string focusTargetName = focusTarget.Name;

                if (focusTargets.Count(t => t.Name == focusTargetName) > 1)
                {
                    focusTargetName += $" ({focusTarget.World})";
                }

                ImGui.Selectable(focusTargetName, false, ImGuiSelectableFlags.None, new Vector2(Item_Width, Item_Height));
                if (ImGui.BeginPopupContextItem())
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

            ImGui.EndListBox();
        }
    }

    private void TargetComboBox()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6, 4));

        ImGui.SetNextItemWidth(200);
        if (ImGui.BeginCombo(" ", comboCurrentValue.Name))
        {
            if (ImGui.Selectable("Focus Target"))
            {
                comboCurrentValue = new PlayerIdentifier("Focus Target", "");
            }

            if (ImGui.Selectable(Helpers.PlayerCharacter.Name + " (you)"))
            {
                comboCurrentValue = Helpers.PlayerCharacter.GetPlayerIdentifier();
            }

            var nearbyPlayers = Helpers.NearbyPlayers.GetNearbyPlayers();

            if (nearbyPlayers.Count > 0)
            {
                ImGui.Separator();

                foreach (var actor in nearbyPlayers)
                {
                    if (ImGui.Selectable(actor.Name.TextValue))
                    {
                        comboCurrentValue = new PlayerIdentifier(actor.Name.TextValue, actor.HomeWorld.Value.Name.ToString() ?? "Unknown World");
                    }
                }
            }

            ImGui.EndCombo();
        }

        ImGui.PopStyleVar();
    }

    private void DrawFocusTargetDisplay(string focusTargets)
    {
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.2f, 0.2f, 0.2f, 1f));
        ImGui.Button(focusTargets, new Vector2(200, Item_Height));
        ImGui.PopStyleColor();
        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.MenuItem("Create Watch Tab From Player"))
            {
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("Remove Player From Group"))
            {

            }

            ImGui.EndPopup();
        }
        // ImGui.EndDisabled();
    }

    private void AddWatchTargetButton(string label, FocusTab focusTab, int width = 0)
    {
        if (ImGui.Button(label, new Vector2(width * Scale, 0)))
        {
            if (comboCurrentValue.Name == "Focus Target")
            {
                var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

                if (focusTarget != null)
                {
                    focusTab.AddFocusTarget(focusTarget);
                }
            }
            else
            {
                focusTab.AddFocusTarget(comboCurrentValue);
            }

            comboCurrentValue = new PlayerIdentifier("Focus Target", "");
        }
    }
}
