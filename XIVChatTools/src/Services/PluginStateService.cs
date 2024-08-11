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

namespace XIVChatTools.Services;

[PluginInterface]
public class PluginStateService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly List<FocusTab> _focusTabs;

    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    public event EventHandler? TestEvent;

    public PluginStateService(Plugin plugin)
    {
        _plugin = plugin;
        _focusTabs = new();
    }

    public void Dispose()
    {

    }

    protected void OnTestEvent(EventArgs e)
    {
        TestEvent?.Invoke(this, e);
    }

    public void TriggerTestEvent(string message) {
        OnTestEvent(new EventArgs());
    }

    public string GetPlayerName()
    {
        return ClientState.LocalPlayer?.Name.TextValue ?? "";
    }

    public List<IPlayerCharacter> GetNearbyPlayers()
    {
        return ObjectTable
          .Where(t => t.Name.TextValue != GetPlayerName() && t.ObjectKind == ObjectKind.Player)
          .Cast<IPlayerCharacter>()
          .OrderBy(t => t.Name.TextValue)
          .ToList();
    }

    public IPlayerCharacter? GetCurrentOrMouseoverTarget()
    {
        IGameObject? focusTarget = TargetManager.Target;

        if (focusTarget == null || focusTarget.ObjectKind != ObjectKind.Player)
        {
            focusTarget = TargetManager.MouseOverTarget;
        }

        if (focusTarget != null && focusTarget.ObjectKind != ObjectKind.Player)
        {
            focusTarget = null;
        }

        return focusTarget as IPlayerCharacter;
    }

    public List<FocusTab> GetFocusTabs()
    {
        return this._focusTabs;
    }

    public void AddFocusTabFromTarget()
    {
        var focusTarget = GetCurrentOrMouseoverTarget();

        if (focusTarget != null)
        {
            var focusTab = new FocusTab(focusTarget.Name.TextValue);

            this._focusTabs.Add(focusTab);
        }
    }

    public void RemoveClosedFocusTabs()
    {
        this._focusTabs.RemoveAll(t => t.Open == false);
    }

    public void ClearAllFocusTabs()
    {
        this._focusTabs.Clear();
    }
}
