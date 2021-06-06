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
    private List<FocusTab> _focusTabs;
    private List<ChatEntry> _chatEntries;
    private DalamudPluginInterface _pluginInterface;
    private Configuration _configuration;

    public static StateManagementRepository Instance { get; private set; }

    private StateManagementRepository(DalamudPluginInterface pi, Configuration config)
    {
      try
      {
        this._chatEntries = new List<ChatEntry>();
        this._focusTabs = new List<FocusTab>();
        this._pluginInterface = pi;
        this._configuration = config;
      }
      catch (Exception e)
      {
        PluginLog.Log(e, "Could not load Chat Scanner!");
      }
    }

    public static void Init(DalamudPluginInterface pi, Configuration config)
    {
      Instance = new StateManagementRepository(pi, config);
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

    public List<ChatEntry> GetAllMessages()
    {
      return this._chatEntries
        .Where(t => t.OwnerId == GetPlayerName())
        .ToList();
    }

    public List<ChatEntry> GetMessagesForFocusTarget()
    {
      var focusTarget = this.GetFocusTarget();

      return this._chatEntries
        .Where(t => t.OwnerId == GetPlayerName())
        .Where(t => t.SenderName == focusTarget?.Name || t.SenderName.StartsWith(focusTarget?.Name))
        .ToList();
    }

    public List<ChatEntry> GetMessagesByPlayerNames(List<string> names)
    {
      return this._chatEntries
        .Where(t => t.OwnerId == GetPlayerName())
        .Where(t => names.Any(name => t.SenderName == name || t.SenderName.StartsWith(name)))
        .ToList();
    }

    public void AddChatMessage(ChatEntry chatEntry)
    {
      chatEntry.OwnerId = GetPlayerName();
      this._chatEntries.Add(chatEntry);

      // PluginLog.Log("Adding chat message to repository");
      // PluginLog.Log("---------------------------------");
      // PluginLog.Log("senderId:" + chatEntry.SenderId);
      // PluginLog.Log("senderName:" + chatEntry.SenderName);
      // PluginLog.Log("message:" + chatEntry.Message);
    }

    public void ClearMessageHistory()
    {
      this._chatEntries.Clear();
    }

    public List<FocusTab> GetFocusTabs()
    {
      return this._focusTabs;
    }

    public void AddFocusTabFromTarget()
    {
      var focusTarget = GetFocusTarget();

      if (focusTarget != null)
      {
        if (_configuration.DebugLogging && _configuration.DebugLoggingCreatingTab)
        {
          PluginLog.Log("CREATING FOCUS TAB");
          PluginLog.Log("=======================================================");
          PluginLog.Log("     Focus Target: " + focusTarget.Name);
          PluginLog.Log("");
          PluginLog.Log("");
          PluginLog.Log("");
          PluginLog.Log("");
        }

        var focusTab = new FocusTab(focusTarget.Name);

        this._focusTabs.Add(focusTab);
      }
    }

    public void RemoveClosedFocusTabs()
    {
      this._focusTabs.RemoveAll(t => t.Open == false);
    }

    public void ClearAllFocusTabs()
    {
      this._focusTabs.Clear();
    }
  }
}
