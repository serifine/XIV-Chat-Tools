using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using Dalamud.Game.Text;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using ChatScanner.Models;

namespace ChatScanner
{
  public class StateManagementRepository : IDisposable
  {
    private List<FocusTab> FocusTabs;
    private List<ChatEntry> ChatEntries;

    private DalamudPluginInterface _pluginInterface;

    public static StateManagementRepository Instance { get; private set; }

    private StateManagementRepository(DalamudPluginInterface pi)
    {
      try
      {
        this.ChatEntries = new List<ChatEntry>();
        this.FocusTabs = new List<FocusTab>();
        this._pluginInterface = pi;
      }
      catch (Exception e)
      {
        PluginLog.Log(e, "Could not load Chat Scanner!");
      }
    }

    public static void Init(DalamudPluginInterface pi)
    {
      Instance = new StateManagementRepository(pi);
    }

    public void Dispose()
    {

    }

    public string GetPlayerName()
    {
      return _pluginInterface.ClientState.LocalPlayer?.Name;
    }

    public List<Actor> GetActorList()
    {
      return _pluginInterface.ClientState.Actors
        .Where(t => t.Name != GetPlayerName() && t.ObjectKind == ObjectKind.Player)
        .OrderBy(t => t.Name)
        .ToList();
    }

    public Actor GetFocusTarget()
    {
      var focusTarget = _pluginInterface.ClientState.Targets.CurrentTarget;

      if (focusTarget == null)
      {
        focusTarget = _pluginInterface.ClientState.Targets.MouseOverTarget;
      }

      if (focusTarget != null && focusTarget.ObjectKind != ObjectKind.Player)
      {
        focusTarget = null;
      }

      return focusTarget;
    }

    public int? GetFocusTargetId()
    {
      return this._pluginInterface.ClientState.Targets.CurrentTarget?.ActorId;
    }

    public void AddChatLog(ChatEntry chatEntry)
    {
      chatEntry.OwnerId = GetPlayerName();
      this.ChatEntries.Add(chatEntry);

      // PluginLog.Log("Adding chat message to repository");
      // PluginLog.Log("---------------------------------");
      // PluginLog.Log("senderId:" + chatEntry.SenderId);
      // PluginLog.Log("senderName:" + chatEntry.SenderName);
      // PluginLog.Log("message:" + chatEntry.Message);
    }

    public List<ChatEntry> GetAllMessages()
    {
      return this.ChatEntries
        .Where(t => t.OwnerId == GetPlayerName())
        .ToList();
    }

    public List<ChatEntry> GetMessagesForFocusTarget()
    {
      var focusTarget = this.GetFocusTarget();

      return this.ChatEntries
        .Where(t => t.OwnerId == GetPlayerName())
        .Where(t => t.SenderName == focusTarget?.Name || t.SenderName.StartsWith(focusTarget?.Name))
        .ToList();
    }

    public List<ChatEntry> GetMessagesByPlayerNames(List<string> names)
    {
      return this.ChatEntries
        .Where(t => t.OwnerId == GetPlayerName())
        .Where(t => names.Any(name => t.SenderName == name || t.SenderName.StartsWith(name)))
        .ToList();
    }

    public void AddFocusTabFromTarget()
    {
      var focusTarget = GetFocusTarget();

      if (focusTarget != null)
      {
        var focusTab = new FocusTab(focusTarget.Name);

        this.FocusTabs.Add(focusTab);
      }
    }

    public void RemoveClosedFocusTabs()
    {
      this.FocusTabs.RemoveAll(t => t.Open == false);
    }

    public List<FocusTab> GetFocusTabs()
    {
      return this.FocusTabs;
    }
  }
}
