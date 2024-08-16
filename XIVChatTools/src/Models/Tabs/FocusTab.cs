using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text;

namespace XIVChatTools.Models.Tabs;

internal class FocusTab : Tab
{
  internal List<string> focusTargets = new List<string>();

  internal FocusTab(string title, string initialTarget) : base(title)
  {
    if (this.focusTargets.Any(t => t == initialTarget) == false)
    {
      this.focusTargets.Add(initialTarget);
    }
  }

  internal List<string> GetFocusTargets()
  {
    return focusTargets.ToList();
  }

  internal void AddFocusTarget(string name)
  {
    if (this.focusTargets.Any(t => t == name) == false)
    {
      this.focusTargets.Add(name);
    }
  }

  internal void RemoveFocusTarget(string name)
  {
    if (this.focusTargets.Any(t => t == name))
    {
      this.focusTargets.Remove(name);
    }
  }

  internal bool IsPlayerAdded(string name)
  {
    return this.focusTargets.Any(t => t == name);
  }
}


