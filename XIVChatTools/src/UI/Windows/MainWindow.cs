

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
    private readonly FocusTargetTabComponent _focusTargetTabComponent;

    private TabControllerService TabController => _plugin.TabController;

    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;
    private MessageService MessageService => _plugin.MessageService;
    private IPluginLog _logger => Plugin.Logger;

    private FocusTabComponent FocusTabComponent;

    internal MainWindow(Plugin plugin) : base($"Chat Tools###ChatToolsMainWindow")
    {
        _plugin = plugin;
        _focusTargetTabComponent = new(_plugin);

        Size = new Vector2(450, 50);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDocking;
        
        FocusTabComponent = new(plugin);
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        
        _focusTargetTabComponent.PreDraw();
        
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
            _focusTargetTabComponent.Draw();
            
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
    }
}
