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
    private List<ChatEntry> chatEntries;
    private List<FocusTab> focusTabs;
    private DalamudPluginInterface _pluginInterface;

    public static StateManagementRepository Instance { get; private set; }

    private StateManagementRepository(DalamudPluginInterface pi)
    {
      try
      {
        this.chatEntries = new List<ChatEntry>();
        this.focusTabs = new List<FocusTab>();
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
      this.chatEntries.Add(chatEntry);

      // PluginLog.Log("Adding chat message to repository");
      // PluginLog.Log("---------------------------------");
      // PluginLog.Log("senderId:" + chatEntry.SenderId);
      // PluginLog.Log("senderName:" + chatEntry.SenderName);
      // PluginLog.Log("message:" + chatEntry.Message);
    }

    public List<ChatEntry> getAllMessages()
    {
      return this.chatEntries.ToList();
    }

    public List<ChatEntry> getMessagesForFocusTarget()
    {
      var name = this.getFocusTargetName();

      return this.chatEntries
        .Where(t => t.SenderName == name || t.SenderName.StartsWith(name))
        .ToList();
    }

    public List<ChatEntry> getMessagesByPlayerNames(List<string> names)
    {
      return this.chatEntries
        .Where(t => names.Any(name => t.SenderName == name || t.SenderName.StartsWith(name)))
        .ToList();
    }

    public void addFocusTabFromTarget()
    {
      if (this._pluginInterface.ClientState.Targets.CurrentTarget != null)
      {
        var focusTab = new FocusTab(this._pluginInterface.ClientState.Targets.CurrentTarget.Name, this._pluginInterface.ClientState.Targets.CurrentTarget.TargetActorID);

        this.focusTabs.Add(focusTab);
      }
    }

    public void removeFocusTab(Guid Id)
    {
      var target = this.focusTabs.Find(t => t.FocusTabId == Id);
      this.focusTabs.Remove(target);
    }

    public List<FocusTab> getFocusTabs()
    {
      return this.focusTabs;
    }
  }
}
