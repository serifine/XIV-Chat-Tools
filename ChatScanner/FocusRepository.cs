using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using Dalamud.Game.Text;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Game.Text.SeStringHandling;
using ChatScanner.Models;

namespace ChatScanner
{
  public class FocusRepository : IDisposable
  {
    private List<FocusTarget> focusTargets;
    private DalamudPluginInterface pi;

    private FocusRepository(DalamudPluginInterface pi)
    {
      try
      {
        this.focusTargets = new List<FocusTarget>();
        this.pi = pi;
      }
      catch (Exception e)
      {
        PluginLog.Log(e, "Could not load Chat Scanner!");
      }
    }

    public void Dispose()
    {

    }

    public static FocusRepository Instance { get; private set; }

    public static void Init(DalamudPluginInterface pi)
    {
      Instance = new FocusRepository(pi);
    }

    public void addFocusTab()
    {
      if (this.pi.ClientState.Targets.CurrentTarget != null && this.focusTargets.Where(t => t.Name != this.pi.ClientState.Targets.CurrentTarget.Name).Count() == 0)
      {
        this.focusTargets.Add(new FocusTarget()
        {
          Id = this.pi.ClientState.Targets.CurrentTarget.TargetActorID,
          Name = this.pi.ClientState.Targets.CurrentTarget.Name
        });
      }
    }

    public string getCurrentFocusTargetName() {
      return this.pi.ClientState.Targets.CurrentTarget?.Name;
    } 

    public int? getCurrentFocusTargetId() {
      return this.pi.ClientState.Targets.CurrentTarget?.ActorId;
    } 

    public void removeFocusTab(int Id)
    {
      var target = this.focusTargets.Find(t => t.Id == Id);
      this.focusTargets.Remove(target);
    }

    public List<FocusTarget> getFocusTargets()
    {
      return this.focusTargets;
    }
  }
}