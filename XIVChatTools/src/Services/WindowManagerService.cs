using System;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using XIVChatTools.UI.Windows;

namespace XIVChatTools.Services;

[PluginInterface]
public class WindowManagerService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly WindowSystem _windowSystem;

    private Configuration Configuration => _plugin.Configuration;

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
