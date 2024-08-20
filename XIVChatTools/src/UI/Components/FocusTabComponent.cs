

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XIVChatTools.Models.Tabs;
using XIVChatTools.Services;

namespace XIVChatTools.UI.Components;


internal class FocusTabComponent
{
    private readonly Plugin _plugin;
    private readonly MessagePanel _messagePanel;

    private readonly float Item_Height = 24f;
    private readonly float Item_Width = 250f;

    private PluginStateService PluginState => _plugin.PluginState;
    private TabControllerService TabController => _plugin.TabController;
    private MessageService MessageService => _plugin.MessageService;
    private IPluginLog Logger => Plugin.Logger;

    private float Scale => ImGui.GetIO().FontGlobalScale;
    private string comboCurrentValue = "Focus Target";

    public FocusTabComponent(Plugin plugin)
    {
        _plugin = plugin;
        _messagePanel = new(plugin);
    }

    internal void Draw(FocusTab focusTab)
    {
        var open = true;
        var focusTargets = focusTab.GetFocusTargets();

        if (ImGui.BeginTabItem(focusTab.Title, ref open))
        {
            DrawFocusTargetHeader(focusTab, focusTargets);


            if (ImGui.BeginTable("table1", 2, ImGuiTableFlags.NoHostExtendX))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 4f));

                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                if (focusTargets.Count > 1)
                {

                    TargetComboBox();
                    ImGui.SameLine();
                    AddWatchTargetButton("Add", focusTab, 50);
                    FocusWatchList(focusTab, focusTargets);
                }

                ImGui.TableSetColumnIndex(1);
                var tabMessages = MessageService.GetMessagesByPlayerNames(focusTargets);

                if (tabMessages.Count > 0)
                {
                    _messagePanel.Draw(tabMessages);
                }
                else
                {
                    ImGui.Text("No messages to display.");
                }

                ImGui.PopStyleVar();

                ImGui.EndTable();
            }


            // ImGui.SameLine();

            // var messages = MessageService.GetAllMessages();
            // _messagePanel.Draw(messages);


            ImGui.EndTabItem();
        }

        if (!open)
        {
            focusTab.Close();
        }
    }

    private void DrawFocusTargetHeader(FocusTab focusTab, List<string> focusTargets)
    {
        if (focusTargets.Count != 1)
        {
            return;
        }

        string tabFocusTarget = focusTargets.First();

        ImGui.BeginChild("##FocusTabHeader", new Vector2(ImGui.GetContentRegionAvail().X, 40 * Scale), true, ImGuiWindowFlags.NoScrollbar);

        DrawFocusTargetDisplay(tabFocusTarget);
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - (297 * Scale));
        TargetComboBox();
        ImGui.SameLine();
        AddWatchTargetButton("Create Group", focusTab);

        ImGui.EndChild();

        ImGui.Separator();
    }

    private void FocusWatchList(FocusTab focusTab, List<string> focusTargets)
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
                ImGui.Selectable(focusTarget, false, ImGuiSelectableFlags.None, new Vector2(Item_Width, Item_Height));
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

        ImGui.SetNextItemWidth(193);
        if (ImGui.BeginCombo(" ", comboCurrentValue))
        {
            if (ImGui.Selectable("Focus Target"))
            {
                comboCurrentValue = "Focus Target";
            }

            if (ImGui.Selectable(Helpers.PlayerCharacter.Name + " (you)"))
            {
                comboCurrentValue = Helpers.PlayerCharacter.Name;
            }

            ImGui.Separator();

            foreach (var actor in Helpers.NearbyPlayers.GetNearbyPlayers())
            {
                if (ImGui.Selectable(actor.Name.TextValue))
                {
                    comboCurrentValue = actor.Name.TextValue;
                }
            }

            ImGui.EndCombo();
        }
    }

    private void DrawFocusTargetDisplay(string focusTarget)
    {
        ImGui.BeginDisabled();
        ImGui.SetNextItemWidth(200);
        ImGui.InputText("", ref focusTarget, 60);
        ImGui.EndDisabled();
    }

    private void AddWatchTargetButton(string label, FocusTab focusTab, int width = 0)
    {
        if (ImGui.Button(label, new Vector2(width * Scale, 0)))
        {
            if (comboCurrentValue == "Focus Target")
            {
                var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

                if (focusTarget != null)
                {
                    focusTab.AddFocusTarget(focusTarget.Name);
                }
            }
            else
            {
                focusTab.AddFocusTarget(comboCurrentValue);
            }

            comboCurrentValue = "Focus Target";
        }
    }
}
