using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.IoC;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Game.Text.SeStringHandling;
using XIVChatTools.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using XIVChatTools.UI.Windows;
using Dalamud.Interface.Windowing;

namespace XIVChatTools.Services;

[PluginInterface]
public class WindowManagerService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly WindowSystem _windowSystem;
    internal readonly PluginUI PluginUI;

    private Configuration Configuration => _plugin.Configuration;

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    public ToolbarWindow ToolbarWindow;
    public SearchWindow SearchWindow;
    public SettingsWindow SettingsWindow;
    public ChatToolsWindow ChatToolsWindow;

    public WindowManagerService(Plugin plugin)
    {
        _plugin = plugin;
        _windowSystem = new(Plugin.Name);

        ToolbarWindow = new(_plugin, this);
        SearchWindow = new(_plugin, this);
        SettingsWindow = new(_plugin, this);
        ChatToolsWindow = new(_plugin, this);

        _windowSystem.AddWindow(ToolbarWindow);
        _windowSystem.AddWindow(SearchWindow);
        _windowSystem.AddWindow(SettingsWindow);
        _windowSystem.AddWindow(ChatToolsWindow);

        PluginUI = new(_plugin);

        ToolbarWindow.IsOpen = Configuration.OpenOnLogin;
    }

    public void Draw()
    {
        PluginUI.Draw();
        _windowSystem.Draw();
    }

    public void CloseAllWindows() {
        ToolbarWindow.IsOpen = false;

        PluginUI.visible = false;
        PluginUI.settingsVisible = false;
    }

    public void Dispose()
    {
        if (PluginUI != null) PluginUI.Dispose();
        
        _windowSystem.RemoveAllWindows();
    }
}
