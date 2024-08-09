

using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XIVChatTools.Services;

namespace XIVChatTools.UI.Windows;

public class SearchWindow : Window
{
    private readonly Plugin _plugin;
    private readonly WindowManagerService _windowManagerService;

    private Configuration Configuration => _plugin.Configuration;
    private float Scale => ImGui.GetIO().FontGlobalScale;

    internal SearchWindow(Plugin plugin, WindowManagerService windowManagerService) : base($"Toolbar###{Plugin.Name}")
    {
        _plugin = plugin;
        _windowManagerService = windowManagerService;

        Size = new Vector2(450, 50);
        Flags = ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoScrollWithMouse
            | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize;
    }

    private void DrawInterface()
    {
    }


    public override void Draw()
    {
        DrawInterface();
    }
}
