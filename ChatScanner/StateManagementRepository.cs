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

    public string getPlayerName()
    {
      return _pluginInterface.ClientState.LocalPlayer?.Name;
    }

    public string getFocusTargetName()
    {
      if (_pluginInterface.ClientState.Targets.CurrentTarget != null)
      {
        return _pluginInterface.ClientState.Targets.CurrentTarget?.Name;
      }

      return _pluginInterface.ClientState.Targets.MouseOverTarget?.Name;
    }

    public int? getFocusTargetId()
    {
      return this._pluginInterface.ClientState.Targets.CurrentTarget?.ActorId;
    }

    public void addChatLog(ChatEntry chatEntry)
    {
      chatEntry.OwnerId = getPlayerName();
      this.ChatEntries.Add(chatEntry);

      // PluginLog.Log("Adding chat message to repository");
      // PluginLog.Log("---------------------------------");
      // PluginLog.Log("senderId:" + chatEntry.SenderId);
      // PluginLog.Log("senderName:" + chatEntry.SenderName);
      // PluginLog.Log("message:" + chatEntry.Message);
    }

    public List<ChatEntry> getAllMessages()
    {
      return this.ChatEntries
        .Where(t => t.OwnerId == getPlayerName())
        .ToList();
    }

    public List<ChatEntry> getMessagesForFocusTarget()
    {
      var name = this.getFocusTargetName();

      return this.ChatEntries
        .Where(t => t.OwnerId == getPlayerName())
        .Where(t => t.SenderName == name || t.SenderName.StartsWith(name))
        .ToList();
    }

    public List<ChatEntry> getMessagesByPlayerNames(List<string> names)
    {
      return this.ChatEntries
        .Where(t => t.OwnerId == getPlayerName())
        .Where(t => names.Any(name => t.SenderName == name || t.SenderName.StartsWith(name)))
        .ToList();
    }

    public void addFocusTabFromTarget()
    {
      if (this._pluginInterface.ClientState.Targets.CurrentTarget != null)
      {
        var focusTab = new FocusTab(this._pluginInterface.ClientState.Targets.CurrentTarget.Name, this._pluginInterface.ClientState.Targets.CurrentTarget.TargetActorID);

        this.FocusTabs.Add(focusTab);
      }
    }

    public void removeFocusTab(Guid Id)
    {
      var target = this.FocusTabs.Find(t => t.FocusTabId == Id);
      this.FocusTabs.Remove(target);
    }

    public List<FocusTab> getFocusTabs()
    {
      return this.FocusTabs;
    }
  }
}
