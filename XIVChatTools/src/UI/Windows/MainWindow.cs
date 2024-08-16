

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XIVChatTools.Models;
using XIVChatTools.Models.Tabs;
using XIVChatTools.Services;

namespace XIVChatTools.UI.Windows;

public class MainWindow : Window
{
    private readonly Plugin _plugin;
    private readonly MessagePanel _messagePanel;

    private TabControllerService TabController => _plugin.TabController;

    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;
    private MessageService MessageService => _plugin.MessageService;
    private IPluginLog Logger => Plugin.Logger;
    
    private float Scale => ImGui.GetIO().FontGlobalScale;

    private string comboCurrentValue = "Focus Target";

    internal MainWindow(Plugin plugin) : base($"Chat Tools###ChatToolsMainWindow")
    {
        _plugin = plugin;
        _messagePanel = new(_plugin);

        this.IsOpen = true;

        Size = new Vector2(450, 50);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDocking;
    }

    public override void Draw()
    {
        ImGui.PopStyleVar();

        try
        {
            DrawInterface();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error drawing Chat Tools window.");
        }
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        base.PreDraw();
    }

    public override void PostDraw()
    {

        base.PostDraw();
    }

    private void DrawInterface()
    {
        if (ImGui.BeginTabBar("ChatToolsTabBar", ImGuiTabBarFlags.NoTooltip))
        {
            ImGui.Text("Hi");
            if (ImGui.BeginTabItem("Focus Group 1"))
            {
                if (ImGui.BeginPopupContextItem()) // <-- use last item id as popup id
                {
                    TabController.GetFocusTabs();
                    string value = string.Empty;
                    ImGui.InputTextWithHint("##tabInput", "Rename Window", ref value, 30);

                    ImGui.BeginDisabled();
                    
                    if (ImGui.Button("Close"))
                        ImGui.CloseCurrentPopup();

                    ImGui.EndDisabled();
                    ImGui.EndPopup();
                }

                float windowHeight = ImGui.GetContentRegionAvail().Y;
                float item_height = ImGui.GetTextLineHeightWithSpacing();
                float item_width = 250f;

                if (ImGui.BeginListBox("##listbox 2", new Vector2(item_width, windowHeight)))
                {
                    ImGui.Selectable("Aureliaux Beladieu", false, ImGuiSelectableFlags.None, new Vector2(item_width, item_height));
                    if (ImGui.BeginPopupContextItem()) // <-- use last item id as popup id
                    {
                        ImGui.MenuItem("Create Watch Tab From Player");
                        if (ImGui.MenuItem("Remove Player From Group"))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                        ImGui.EndPopup();
                    }

                    ImGui.Selectable("Tessa Elran (you)", false, ImGuiSelectableFlags.None, new Vector2(item_width, item_height));
                    if (ImGui.BeginPopupContextItem()) // <-- use last item id as popup id
                    {
                        ImGui.Text("This a popup 2");
                        if (ImGui.Button("Close"))
                            ImGui.CloseCurrentPopup();
                        ImGui.EndPopup();
                    }

                    ImGui.Selectable("Item 3", false, ImGuiSelectableFlags.None, new Vector2(item_width, item_height));
                    // if (ImGui.IsItemHovered()) {
                    //     ImGui.SetTooltip("Item 3 is hovered");
                    // }

                    if (ImGui.BeginPopupContextItem()) // <-- use last item id as popup id
                    {
                        ImGui.Text("This a popup");
                        if (ImGui.Button("Close"))
                            ImGui.CloseCurrentPopup();
                        ImGui.EndPopup();
                    }

                    ImGui.EndListBox();
                }

                ImGui.SameLine();

                var messages = MessageService.GetAllMessages();
                _messagePanel.Draw(messages);

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Current Target"))
            {
                DrawSelectedTargetTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabItem();
        }
        // var currentItem = -1;

        // ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);

        // ImGui.ListBox("", ref currentItem, ["Hi", "Hello"], 2, 5);

        // ImGui.PopStyleVar();


        // Custom size: use all width, 5 items tall


        // ImGui.TabItemButton("Test", ImGuiTabItemFlags.NoCloseWithMiddleMouseButton);

        // DrawSelectedTargetTab();
        // DrawCustomTab("DefaultCustom");

        // uint dockspaceId = ImGui.GetID("ChatToolsDockspace");
        // ImGui.DockSpace(dockspaceId);


        // foreach (var focusTab in PluginState.GetFocusTabs())
        // {
        //     ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.Appearing);
        //     if (ImGui.Begin(focusTab.Name, ref focusTab.Open))
        //     {
        //         DrawFocusTab(focusTab.FocusTabId.ToString(), focusTab);

        //         ImGui.End();
        //     }
        // }

        // PluginState.RemoveClosedFocusTabs();
    }

    private void DrawSelectedTargetTab()
    {
        var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

        if (focusTarget == null)
        {
            ImGui.Text("No target hovered or selected.");
            return;
        }

        var messages = MessageService.GetMessagesForFocusTarget();

        if (messages != null && messages.Count > 0)
        {
            _messagePanel.Draw(messages);
        }
        else
        {
            ImGui.Text("No messages found for " + focusTarget.Name + ".");
        }
    }

    private void DrawFocusTab(string tabId, FocusTab focusTab)
    {
        var tabMessages = MessageService.GetMessagesByPlayerNames(focusTab.GetFocusTargets());

        if (tabMessages.Count > 0)
        {
            _messagePanel.Draw(tabMessages);
        }
        else
        {
            ImGui.Text("No messages to display.");
        }
    }

    // private void DrawCustomTab(string tabId)
    // {
    //     if (ImGui.BeginTable("table1", 2, ImGuiTableFlags.NoHostExtendX))
    //     {
    //         ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
    //         ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
    //         foreach (var name in customWindowFocusTab.GetFocusTargets())
    //         {
    //             ImGui.TableNextRow();
    //             ImGui.TableSetColumnIndex(0);

    //             if (ImGui.SmallButton("Remove##" + tabId + name))
    //             {
    //                 customWindowFocusTab.RemoveFocusTarget(name);
    //             }

    //             ImGui.TableSetColumnIndex(1);
    //             ImGui.Text(name);
    //         }
    //         ImGui.EndTable();
    //     }


    //     ImGui.SameLine(ImGui.GetContentRegionAvail().X - (300 * Scale));
    //     ImGui.SetNextItemWidth(200);
    //     if (ImGui.BeginCombo(" ", comboCurrentValue))
    //     {
    //         if (ImGui.Selectable("Focus Target"))
    //         {
    //             comboCurrentValue = "Focus Target";
    //         }

    //         if (ImGui.Selectable(PluginState.GetPlayerName() + " (you)"))
    //         {
    //             // focusTab.AddFocusTarget(StateRepository.GetPlayerName());
    //             comboCurrentValue = PluginState.GetPlayerName();
    //         }

    //         ImGui.Separator();

    //         foreach (var actor in PluginState.GetNearbyPlayers())
    //         {
    //             if (ImGui.Selectable(actor.Name.TextValue))
    //             {
    //                 comboCurrentValue = actor.Name.TextValue;
    //             }
    //         }

    //         ImGui.EndCombo();
    //     }
    //     ImGui.SameLine();
    //     if (ImGui.Button("Add To Group"))
    //     {
    //         if (comboCurrentValue == "Focus Target")
    //         {
    //             var focusTarget = PluginState.GetCurrentOrMouseoverTarget();

    //             if (focusTarget != null)
    //             {
    //                 customWindowFocusTab.AddFocusTarget(focusTarget.Name.TextValue);
    //             }
    //         }
    //         else
    //         {
    //             customWindowFocusTab.AddFocusTarget(comboCurrentValue);
    //         }

    //         comboCurrentValue = "Focus Target";
    //     }

    //     ImGui.Separator();

    //     var tabMessages = MessageService.GetMessagesByPlayerNames(customWindowFocusTab.GetFocusTargets());

    //     if (tabMessages.Count() > 0)
    //     {
    //         _messagePanel.Draw(tabMessages);
    //     }
    //     else
    //     {
    //         ImGui.Text("No messages to display.");
    //     }
    // }
}
