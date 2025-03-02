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
using System.Runtime.CompilerServices;

namespace XIVChatTools.Services;

[PluginInterface]
public class TabControllerService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly List<Tab> _tabs = new();

    public TabControllerService(Plugin plugin)
    {
        _plugin = plugin;

        var player = Helpers.PlayerCharacter.GetPlayerIdentifier();
        AddFocusTab(player);
    }

    public void Dispose()
    {

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
            AddFocusTab(focusTarget);
        }
    }

    internal void AddFocusTab(PlayerIdentifier target)
    {
        string tabName = target.Name;

        if (_tabs.Any(t => t.Title == tabName))
        {
            tabName = $"{tabName} (2)";
        }
    }

    internal void PostDrawEvents()
    {
        List<Tab> tabsToClose = this._tabs.FindAll(t => t.ShouldCloseNextFrame);

        foreach (var tab in tabsToClose)
        {
            tab.Dispose();
            this._tabs.Remove(tab);
        }
    }

    internal void ClearAllTabs()
    {
        this._tabs.Clear();
    }
}
