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
using System.Collections.ObjectModel;
using XIVChatTools.Models.Tabs;

namespace XIVChatTools.Services;

[PluginInterface]
public class TabControllerService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly List<Tab> _tabs = new();

    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    internal event EventHandler? TestEvent;

    public TabControllerService(Plugin plugin)
    {
        _plugin = plugin;
        
        FocusTab focusTab = new("Aureliaux Beladieu");

        focusTab.AddFocusTarget("Tessa Elran");

        _tabs.Add(focusTab);
    }

    public void Dispose()
    {

    }

    protected void OnTestEvent(EventArgs e)
    {
        TestEvent?.Invoke(this, e);
    }

    internal void TriggerTestEvent(string message) {
        OnTestEvent(new EventArgs());
    }

    internal List<FocusTab> GetFocusTabs()
    {
        return this._tabs.OfType<FocusTab>().ToList();
    }

    internal void AddFocusTabFromTarget()
    {
        var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

        if (focusTarget != null)
        {
            var focusTab = new FocusTab(focusTarget.Name.TextValue);

            this._tabs.Add(focusTab);
        }
    }

    internal void AddFocusTab(string name)
    {
        var focusTab = new FocusTab(name);

        this._tabs.Add(focusTab);
    }

    internal void PostDrawEvents()
    {
        this._tabs.RemoveAll(t => t.ShouldCloseNextFrame);
    }

    internal void ClearAllTabs()
    {
        this._tabs.Clear();
    }
}
