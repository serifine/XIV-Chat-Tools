using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.IoC;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Game.Text.SeStringHandling;
using ChatScanner.Models;

namespace ChatScanner
{
  [PluginInterface]
  public class PluginState : IDisposable
  {
    private List<FocusTab> _focusTabs { get; set; }
    private List<ChatEntry> _chatEntries { get; set; }

    private Configuration Configuration { get; set; }
    
    [PluginService]
    internal DalamudPluginInterface PluginInterface { get; set; }
    
    [PluginService]
    internal ClientState ClientState { get; set; }
    
    [PluginService]
    internal ObjectTable ObjectTable { get; set; }
    
    [PluginService]
    internal TargetManager TargetManager { get; init; }

    // public static StateManagementRepository Instance { get; private set; }

    public PluginState(Configuration config)
    {
      try
      {
        this._chatEntries = new List<ChatEntry>();
        this._focusTabs = new List<FocusTab>();
        this.Configuration = config;
      }
      catch (Exception e)
      {
        PluginLog.Log(e, "Could not load Chat Scanner!");
      }
    }

    public void Dispose()
    {
      
    }

    public string GetPlayerName()
    {
      return this.ClientState.LocalPlayer.Name.TextValue;
    }

    public List<GameObject> GetActorList()
    {
      return this.ObjectTable
        .Where(t => t.Name.TextValue != GetPlayerName() && t.ObjectKind == ObjectKind.Player)
        .OrderBy(t => t.Name)
        .ToList();
    }

    public GameObject? GetFocusTarget()
    {
      GameObject? focusTarget = this.TargetManager.Target;

      if (focusTarget == null || focusTarget.ObjectKind != ObjectKind.Player)
      {
        focusTarget = TargetManager.MouseOverTarget;
      }

      if (focusTarget != null && focusTarget.ObjectKind != ObjectKind.Player)
      {
        focusTarget = null;
      }

      return focusTarget;
    }

    public int? GetFocusTargetId()
    {
      return ((int)this.GetFocusTarget().ObjectId);
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
        .Where(t => t.SenderName == focusTarget?.Name.TextValue || t.SenderName.StartsWith(focusTarget?.Name.TextValue))
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

      PluginLog.Log("Adding chat message to repository");
      PluginLog.Log("---------------------------------");
      PluginLog.Log("senderId:" + chatEntry.SenderId);
      PluginLog.Log("senderName:" + chatEntry.SenderName);
      PluginLog.Log("message:" + chatEntry.Message);
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
        if (Configuration.DebugLogging && Configuration.DebugLoggingCreatingTab)
        {
          PluginLog.Log("CREATING FOCUS TAB");
          PluginLog.Log("=======================================================");
          PluginLog.Log("     Focus Target: " + focusTarget.Name);
          PluginLog.Log("");
          PluginLog.Log("");
          PluginLog.Log("");
          PluginLog.Log("");
        }

        var focusTab = new FocusTab(focusTarget.Name.TextValue);

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
