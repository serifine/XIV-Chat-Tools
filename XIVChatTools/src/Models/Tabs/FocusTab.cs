using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text;

namespace XIVChatTools.Models.Tabs;

internal class FocusTab : Tab
{
    private List<PlayerIdentifier> focusTargets = new List<PlayerIdentifier>();

    internal FocusTab(PlayerIdentifier initialTarget, string title = "New Watcher") : base(title)
    {
        if (this.focusTargets.Any(t => t.Equals(initialTarget)) == false)
        {
            this.focusTargets.Add(initialTarget);
        }
    }

    internal List<PlayerIdentifier> GetFocusTargets()
    {
        return focusTargets.ToList();
    }

    internal void AddFocusTarget(PlayerIdentifier target)
    {
        if (this.focusTargets.Any(t => t.Equals(target)) == false)
        {
            this.focusTargets.Add(target);
        }
    }

    internal void RemoveFocusTarget(PlayerIdentifier target)
    {
        var selectedTarget = this.focusTargets.FirstOrDefault(t => t.Equals(target));

        if (selectedTarget != null)
        {
            this.focusTargets.Remove(selectedTarget);
        }
    }

    internal bool IsPlayerAdded(PlayerIdentifier target)
    {
        return this.focusTargets.Any(t => t.Equals(target));
    }
}


