using System;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using ChatTools.UI;
using ChatTools.UI.Windows;

namespace ChatTools.Services;

[PluginInterface]
public class WindowManagerService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly WindowSystem _windowSystem;

    private Configuration Configuration => _plugin.Configuration;

    public SearchWindow SearchWindow;
    public SettingsWindow SettingsWindow;
    public MainWindow MainWindow;

    public WindowManagerService(Plugin plugin)
    {
        _plugin = plugin;
        _windowSystem = new(Plugin.Name);

        SearchWindow = new(_plugin);
        SettingsWindow = new(_plugin);
        MainWindow = new(_plugin);

        _windowSystem.AddWindow(SearchWindow);
        _windowSystem.AddWindow(SettingsWindow);
        _windowSystem.AddWindow(MainWindow);

        MainWindow.IsOpen = Plugin.ClientState.IsLoggedIn && Configuration.OpenOnLogin;

        // REMOVE
        SettingsWindow.IsOpen = true;
    }

    public void Draw()
    {
        StyleManager.ApplyStyles();
        _windowSystem.Draw();
        StyleManager.RemoveStyles();
    }

    public void CloseAllWindows() {
        SearchWindow.IsOpen = false;
        SettingsWindow.IsOpen = false;
        MainWindow.IsOpen = false;
    }

    public void Dispose()
    {        
        _windowSystem.RemoveAllWindows();
    }
}
