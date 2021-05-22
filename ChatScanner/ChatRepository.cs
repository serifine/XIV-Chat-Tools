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
  public class ChatRepository : IDisposable
  {
    private List<ChatEntry> chatEntries;
    private DalamudPluginInterface pi;

    private ChatRepository(DalamudPluginInterface pi)
    {
      try
      {
        this.chatEntries = new List<ChatEntry>();
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

    public static ChatRepository Instance { get; private set; }

    public static void Init(DalamudPluginInterface pi)
    {
      Instance = new ChatRepository(pi);
    }

    public void addChatLog(ChatEntry chatEntry)
    {
      this.chatEntries.Add(chatEntry);

      PluginLog.Log("Adding chat message to repository");
      PluginLog.Log("senderId:" + chatEntry.SenderId);
      PluginLog.Log("senderName:" + chatEntry.SenderName);
      PluginLog.Log("message:" + chatEntry.Message);
    }

    public List<ChatEntry> getAllLogs()
    {
      return this.chatEntries;
    }

    public List<ChatEntry> getByName(string name) {
      return this.chatEntries.Where(t => t.SenderName == name || t.SenderName.StartsWith(name)).ToList();
    }

    public string getPlayerName() {
      return pi.ClientState.LocalPlayer?.Name;
    }

    public int? getPlayerId() {
      return pi.ClientState.LocalPlayer?.ActorId;
    }
  }
}