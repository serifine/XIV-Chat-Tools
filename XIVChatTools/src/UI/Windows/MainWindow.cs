

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
using XIVChatTools.UI.Components;

namespace XIVChatTools.UI.Windows;

public class MainWindow : Window
{
    private readonly Plugin _plugin;
    private readonly MessagePanel _messagePanel;

    private TabControllerService TabController => _plugin.TabController;

    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;
    private MessageService MessageService => _plugin.MessageService;
    private IPluginLog _logger => Plugin.Logger;

    private FocusTabComponent FocusTabComponent;

    internal MainWindow(Plugin plugin) : base($"Chat Tools###ChatToolsMainWindow")
    {
        _plugin = plugin;
        _messagePanel = new(_plugin);

        this.IsOpen = true;

        Size = new Vector2(450, 50);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDocking;
        
        FocusTabComponent = new(plugin);
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        base.PreDraw();
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
            _logger.Error(e, "Error drawing Chat Tools window.");
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();
    }

    private void DrawInterface()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 0, 0));
        if (ImGui.BeginTabBar("ChatToolsTabBar", ImGuiTabBarFlags.NoTooltip | ImGuiTabBarFlags.Reorderable))
        {
            if (ImGui.BeginTabItem("Current Target"))
            {
                DrawSelectedTargetTab();
                ImGui.EndTabItem();
            }
            
            foreach (var tab in TabController.GetFocusTabs())
            {
                FocusTabComponent.Draw(tab);
            }

            ImGui.EndTabBar();
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }

    private void DrawSelectedTargetTab()
    {
        var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

        if (focusTarget == null)
        {
            ImGui.Text("No target hovered or selected.");
            return;
        }

        var messages = MessageService.GetMessagesForFocusTarget(focusTarget);

        if (messages != null && messages.Count > 0)
        {
            _messagePanel.Draw(messages);
        }
        else
        {
            ImGui.Text("No messages found for " + focusTarget.Name + ".");
        }
    }
}
