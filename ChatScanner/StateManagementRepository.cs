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

    public string GetFocusTargetName()
    {
      if (_pluginInterface.ClientState.Targets.CurrentTarget != null)
      {
        return _pluginInterface.ClientState.Targets.CurrentTarget?.Name;
      }

      return _pluginInterface.ClientState.Targets.MouseOverTarget?.Name;
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
      var name = this.GetFocusTargetName();

      return this.ChatEntries
        .Where(t => t.OwnerId == GetPlayerName())
        .Where(t => t.SenderName == name || t.SenderName.StartsWith(name))
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
      if (this._pluginInterface.ClientState.Targets.CurrentTarget != null)
      {
        var focusTab = new FocusTab(this._pluginInterface.ClientState.Targets.CurrentTarget.Name, this._pluginInterface.ClientState.Targets.CurrentTarget.TargetActorID);

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
