using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text;

namespace XIVChatTools.Models
{
  public class FocusTab
  {
    public Guid FocusTabId = new Guid();
    public string Name = "";
    public bool Open = true;
    public List<string> focusTargets = new List<string>();

    public FocusTab(string name)
    {
      this.Name = name;

      if (this.focusTargets.Any(t => t == name) == false)
      {
        this.focusTargets.Add(name);
      }
    }

    public List<string> GetFocusTargets()
    {
      return focusTargets.ToList();
    }

    public void AddFocusTarget(string name)
    {
      if (this.focusTargets.Any(t => t == name) == false)
      {
        this.focusTargets.Add(name);
      }
    }

    public void RemoveFocusTarget(string name)
    {
      if (this.focusTargets.Any(t => t == name))
      {
        this.focusTargets.Remove(name);
      }
    }

    public bool IsPlayerAdded(string name) {
      return this.focusTargets.Any(t => t == name);
    }
  }

  public class FocusTarget
  {
    public int Id { get; set; }
    public required string Name { get; set; }
  }
}
