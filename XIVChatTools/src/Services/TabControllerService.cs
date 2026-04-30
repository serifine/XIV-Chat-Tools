using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.IoC;
using XIVChatTools.Models;
using XIVChatTools.Models.Tabs;

namespace XIVChatTools.Services;

[PluginInterface]
public class TabControllerService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly List<Tab> _tabs = new();

    public TabControllerService(Plugin plugin)
    {
        _plugin = plugin;
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

    internal void AddFocusTab(PlayerIdentifier? target = null)
    {
        if (target == null)
        {
            this._tabs.Add(new FocusTab(_plugin));
        }
        else
        {
            string tabName = target.Name;

            this._tabs.Add(new FocusTab(_plugin, target, tabName));
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
