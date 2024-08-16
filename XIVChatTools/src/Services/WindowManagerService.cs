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

    private Configuration Configuration => _plugin.Configuration;

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    public ToolbarWindow ToolbarWindow;
    public SearchWindow SearchWindow;
    public SettingsWindow SettingsWindow;
    public MainWindow MainWindow;

    public WindowManagerService(Plugin plugin)
    {
        _plugin = plugin;
        _windowSystem = new(Plugin.Name);

        ToolbarWindow = new(_plugin);
        SearchWindow = new(_plugin);
        SettingsWindow = new(_plugin);
        MainWindow = new(_plugin);

        _windowSystem.AddWindow(ToolbarWindow);
        _windowSystem.AddWindow(SearchWindow);
        _windowSystem.AddWindow(SettingsWindow);
        _windowSystem.AddWindow(MainWindow);

        ToolbarWindow.IsOpen = Plugin.ClientState.IsLoggedIn && Configuration.OpenOnLogin;
    }

    public void Draw()
    {
        _windowSystem.Draw();
    }

    public void CloseAllWindows() {
        SearchWindow.IsOpen = false;
        SettingsWindow.IsOpen = false;
        MainWindow.IsOpen = false;
        ToolbarWindow.IsOpen = false;
    }

    public void Dispose()
    {        
        _windowSystem?.RemoveAllWindows();
    }
}
