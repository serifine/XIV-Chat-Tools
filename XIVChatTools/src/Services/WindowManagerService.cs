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
    private readonly WindowSystem WindowSystem;
    internal readonly PluginUI PluginUI;

    private Configuration Configuration => _plugin.Configuration;

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    public ToolbarWindow ToolbarWindow;

    public WindowManagerService(Plugin plugin)
    {
        _plugin = plugin;
        WindowSystem = new(Plugin.Name);

        ToolbarWindow = new(_plugin, this);

        WindowSystem.AddWindow(ToolbarWindow);

        PluginUI = new(_plugin);

        ToolbarWindow.IsOpen = Configuration.OpenOnLogin;
    }

    public void Draw()
    {
        PluginUI.Draw();
        WindowSystem.Draw();
    }

    public void CloseAllWindows() {
        ToolbarWindow.IsOpen = false;

        PluginUI.visible = false;
        PluginUI.settingsVisible = false;
    }

    public void Dispose()
    {
        if (PluginUI != null) PluginUI.Dispose();
        
        WindowSystem.RemoveAllWindows();
    }
}
