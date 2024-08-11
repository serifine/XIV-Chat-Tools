

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
using XIVChatTools.Services;

namespace XIVChatTools.UI.Windows;

public class MainWindow : Window
{
    private readonly Plugin _plugin;
    private readonly WindowManagerService _windowManagerService;
    private readonly MessagePanel _messagePanel;

    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;
    private MessageService MessageService => _plugin.MessageService;
    private IPluginLog Logger => Plugin.Logger;
    private float Scale => ImGui.GetIO().FontGlobalScale;
    private FocusTab customWindowFocusTab = new FocusTab("Private Window")
    {
        focusTargets = new List<string>()
    };

    private string comboCurrentValue = "Focus Target";

    internal MainWindow(Plugin plugin, WindowManagerService windowManagerService) : base($"Chat Tools###ChatToolsMainWindow")
    {
        _plugin = plugin;
        _windowManagerService = windowManagerService;
        _messagePanel = new(_plugin);

        this.IsOpen = true;

        Size = new Vector2(450, 50);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground;
    }

    public void DrawSelectedTargetTab()
    {
        var focusTarget = PluginState.GetCurrentOrMouseoverTarget();

        if (focusTarget != null)
        {
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
        else
        {
            ImGui.Text("No target selected.");
        }
    }

    public void DrawFocusTab(string tabId, FocusTab focusTab)
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

    public void DrawCustomTab(string tabId)
    {
        if (ImGui.BeginTable("table1", 2, ImGuiTableFlags.NoHostExtendX))
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
            foreach (var name in customWindowFocusTab.GetFocusTargets())
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                if (ImGui.SmallButton("Remove##" + tabId + name))
                {
                    customWindowFocusTab.RemoveFocusTarget(name);
                }

                ImGui.TableSetColumnIndex(1);
                ImGui.Text(name);
            }
            ImGui.EndTable();
        }


        ImGui.SameLine(ImGui.GetContentRegionAvail().X - (300 * Scale));
        ImGui.SetNextItemWidth(200);
        if (ImGui.BeginCombo(" ", comboCurrentValue))
        {
            if (ImGui.Selectable("Focus Target"))
            {
                comboCurrentValue = "Focus Target";
            }

            if (ImGui.Selectable(PluginState.GetPlayerName() + " (you)"))
            {
                // focusTab.AddFocusTarget(StateRepository.GetPlayerName());
                comboCurrentValue = PluginState.GetPlayerName();
            }

            ImGui.Separator();

            foreach (var actor in PluginState.GetNearbyPlayers())
            {
                if (ImGui.Selectable(actor.Name.TextValue))
                {
                    comboCurrentValue = actor.Name.TextValue;
                }
            }

            ImGui.EndCombo();
        }
        ImGui.SameLine();
        if (ImGui.Button("Add To Group"))
        {
            if (comboCurrentValue == "Focus Target")
            {
                var focusTarget = PluginState.GetCurrentOrMouseoverTarget();

                if (focusTarget != null)
                {
                    customWindowFocusTab.AddFocusTarget(focusTarget.Name.TextValue);
                }
            }
            else
            {
                customWindowFocusTab.AddFocusTarget(comboCurrentValue);
            }

            comboCurrentValue = "Focus Target";
        }

        ImGui.Separator();

        var tabMessages = MessageService.GetMessagesByPlayerNames(customWindowFocusTab.GetFocusTargets());

        if (tabMessages.Count() > 0)
        {
            _messagePanel.Draw(tabMessages);
        }
        else
        {
            ImGui.Text("No messages to display.");
        }
    }

    private void DrawInterface()
    {
        uint dockspaceId = ImGui.GetID("ChatToolsDockspace");
        ImGui.DockSpace(dockspaceId);

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.Appearing);
        if (ImGui.Begin("Selected Target"))
        {
            DrawSelectedTargetTab();
            ImGui.End();
        }

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.Appearing);
        if (ImGui.Begin("Custom Watch"))
        {
            DrawCustomTab("DefaultCustom");
            ImGui.End();
        }

        foreach (var focusTab in PluginState.GetFocusTabs())
        {
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.Appearing);
            if (ImGui.Begin(focusTab.Name, ref focusTab.Open))
            {
                DrawFocusTab(focusTab.FocusTabId.ToString(), focusTab);

                ImGui.End();
            }
        }

        PluginState.RemoveClosedFocusTabs();
    }


    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        base.PreDraw();
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar();

        base.PostDraw();
    }

    public override void Draw()
    {
        try
        {
            DrawInterface();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error drawing Chat Tools window.");
        }
    }
}
