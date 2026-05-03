using System;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using XIVChatTools.UI;
using XIVChatTools.UI.Windows;

namespace XIVChatTools.Services;

[PluginInterface]
public class WindowManagerService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly WindowSystem _windowSystem;
    private readonly StyleManager _styleManager;

    private Configuration Configuration => _plugin.Configuration;

    public SearchWindow SearchWindow;
    public SettingsWindow SettingsWindow;
    public MainWindow MainWindow;

    public WindowManagerService(Plugin plugin)
    {
        _plugin = plugin;
        _windowSystem = new(Plugin.Name);
        _styleManager = new StyleManager();

        SearchWindow = new(_plugin);
        SettingsWindow = new(_plugin);
        MainWindow = new(_plugin);

        _windowSystem.AddWindow(SearchWindow);
        _windowSystem.AddWindow(SettingsWindow);
        _windowSystem.AddWindow(MainWindow);

        MainWindow.IsOpen = Plugin.ClientState.IsLoggedIn && Configuration.OpenOnLogin;
    }

    public void Draw()
    {
        _styleManager.ApplyStyles();
        _windowSystem.Draw();
        _styleManager.RemoveStyles();
    }

    public void CloseAllWindows() {
        SearchWindow.IsOpen = false;
        SettingsWindow.IsOpen = false;
        MainWindow.IsOpen = false;
    }

    public void Dispose()
    {        
        _styleManager.Dispose();
        _windowSystem.RemoveAllWindows();
    }
}
