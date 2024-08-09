

using System;
using System.Collections.Generic;
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

public class SearchWindow : Window
{
    private readonly Plugin _plugin;
    private readonly WindowManagerService _windowManagerService;
    private readonly MessagePanel _messagePanel;

    private PluginStateService PluginState => _plugin.PluginState;

    private List<ChatEntry> searchMessages = [];
    private string searchText = "";

    internal SearchWindow(Plugin plugin, WindowManagerService windowManagerService) : base($"Search###ChatToolsSearchWindow")
    {
        _plugin = plugin;
        _windowManagerService = windowManagerService;
        _messagePanel = new(_plugin);

        searchMessages = PluginState.GetAllMessages();

        Size = new Vector2(450, 600);
        SizeConstraints = new WindowSizeConstraints() { MinimumSize = new Vector2(450, 600), MaximumSize = new Vector2(700, 1200) };
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDocking;
    }

    private void DrawInterface()
    {
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputTextWithHint("", "Search Messages", ref searchText, 24096))
        {
            searchMessages = PluginState.SearchMessages(searchText);
        }

        ImGui.Separator();

        if (searchMessages.Count > 0)
        {
            _messagePanel.Draw(searchMessages);
        }
        else
        {
            ImGui.Text("No messages found.");
        }
    }

    public override void Draw()
    {
        DrawInterface();
    }
}
