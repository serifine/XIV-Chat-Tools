using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using ChatScanner.Models;
using Dalamud.Game.Text;

namespace ChatScanner
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  class PluginUI : IDisposable
  {
    private Configuration configuration;
    private StateManagementRepository StateRepository;

    // this extra bool exists for ImGui, since you can't ref a property
    private bool visible = false;
    public bool Visible
    {
      get { return this.visible; }
      set { this.visible = value; }
    }

    private bool settingsVisible = false;
    public bool SettingsVisible
    {
      get { return this.settingsVisible; }
      set { this.settingsVisible = value; }
    }


    private bool autoScrollToBottom = false;
    public bool AutoScrollToBottom
    {
      get { return this.autoScrollToBottom; }
      set { this.autoScrollToBottom = value; }
    }

    private string comboCurrentValue = "Focus Target";

    private readonly Vector4 ORANGE_COLOR = new Vector4(0.950f, 0.500f, 0f, 1f);
    private readonly Vector4 LIGHT_ORANGE_COLOR = new Vector4(0.950f, 0.650f, 0f, 1f);

    private FocusTab customWindowFocusTab = new FocusTab("Private Window")
    {
      focusTargets = new List<string>()
    };

    // passing in the image here just for simplicity
    public PluginUI(Configuration configuration)
    {
      this.configuration = configuration;
      this.StateRepository = StateManagementRepository.Instance;
    }

    public void Dispose()
    {
    }

    public void Draw()
    {
      DrawMainWindow();
      DrawSettingsWindow();
    }

    public void DrawMainWindow()
    {
      if (!Visible)
      {
        return;
      }

      ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ORANGE_COLOR);
      ImGui.PushStyleColor(ImGuiCol.CheckMark, ORANGE_COLOR);

      ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
      ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));

      var scale = ImGui.GetIO().FontGlobalScale;

      if (ImGui.Begin("Chat Scanner", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
      {
        ImGui.Checkbox("Auto scroll on new messages.", ref autoScrollToBottom);
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - (100 * scale));
        if (ImGui.Button("Add Focus Target"))
        {
          StateRepository.AddFocusTabFromTarget();
        }

        // ImGui.Separator();

        if (ImGui.BeginTabBar("MainTabs", ImGuiTabBarFlags.Reorderable))
        {
          if (ImGui.BeginTabItem("Selected Target"))
          {
            SelectedTargetTab();

            ImGui.EndTabItem();
          }

          if (ImGui.BeginTabItem("All Messages"))
          {
            AllMessagesTab();

            ImGui.EndTabItem();
          }

          if (ImGui.BeginTabItem("Custom Watch"))
          {
            DrawCustomTab("DefaultCustom");

            ImGui.EndTabItem();
          }

          foreach (var focusTab in StateRepository.GetFocusTabs())
          {
            if (ImGui.BeginTabItem(focusTab.Name, ref focusTab.Open, ImGuiTabItemFlags.None))
            {
              DrawFocusTab(focusTab.FocusTabId.ToString(), focusTab);

              ImGui.EndTabItem();
            }
          }

          StateRepository.RemoveClosedFocusTabs();

          ImGui.EndTabBar();
        }
      }
      ImGui.End();
    }


    public void SelectedTargetTab()
    {
      var focusTarget = StateRepository.GetFocusTarget();

      if (focusTarget != null)
      {
        var messages = StateRepository.GetMessagesForFocusTarget();

        if (messages != null && messages.Count() > 0)
        {
          MessagePanel(messages);
        }
        else
        {
          ImGui.Text("No messages found for " + focusTarget.Name + ".");
        }
      }
      else
      {
        ImGui.Text("No target selected.");
      }
    }

    public void AllMessagesTab()
    {

      var tabMessages = StateRepository.GetAllMessages();

      if (tabMessages.Count() > 0)
      {
        MessagePanel(tabMessages);
      }
      else
      {
        ImGui.Text("No messages to display.");
      }
    }

    public void DrawCustomTab(string tabId)
    {
      var scale = ImGui.GetIO().FontGlobalScale;

      if (ImGui.BeginTable("table1", 2, ImGuiTableFlags.NoHostExtendX))
      {
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        foreach (var name in customWindowFocusTab.GetFocusTargets())
        {
          ImGui.TableNextRow();
          ImGui.TableSetColumnIndex(0);

          if (ImGui.SmallButton("Remove##" + tabId + name))
          {
            customWindowFocusTab.RemoveFocusTarget(name);
          }

          ImGui.TableSetColumnIndex(1);
          ImGui.Text(name);
        }
        ImGui.EndTable();
      }


      ImGui.SameLine(ImGui.GetContentRegionAvail().X - (300 * scale));
      ImGui.SetNextItemWidth(200);
      if (ImGui.BeginCombo(" ", comboCurrentValue))
      {
        if (ImGui.Selectable("Focus Target"))
        {
          comboCurrentValue = "Focus Target";
        }

        if (ImGui.Selectable(StateRepository.GetPlayerName() + " (you)"))
        {
          // focusTab.AddFocusTarget(StateRepository.GetPlayerName());
          comboCurrentValue = StateRepository.GetPlayerName();
        }

        ImGui.Separator();

        foreach (var actor in StateRepository.GetActorList())
        {
          if (ImGui.Selectable(actor.Name))
          {
            comboCurrentValue = actor.Name;
          }
        }

        ImGui.EndCombo();
      }
      ImGui.SameLine();
      if (ImGui.Button("Add To Group"))
      {
        if (comboCurrentValue == "Focus Target")
        {
          var focusTarget = StateRepository.GetFocusTarget();

          if (focusTarget != null)
          {
            customWindowFocusTab.AddFocusTarget(focusTarget.Name);
          }
        }
        else
        {
          customWindowFocusTab.AddFocusTarget(comboCurrentValue);
        }

        comboCurrentValue = "Focus Target";
      }

      ImGui.Separator();

      var tabMessages = StateRepository.GetMessagesByPlayerNames(customWindowFocusTab.GetFocusTargets());

      if (tabMessages.Count() > 0)
      {
        MessagePanel(tabMessages);
      }
      else
      {
        ImGui.Text("No messages to display.");
      }
    }

    public void DrawFocusTab(string tabId, FocusTab focusTab)
    {

      var tabMessages = StateRepository.GetMessagesByPlayerNames(focusTab.GetFocusTargets());

      if (tabMessages.Count() > 0)
      {
        MessagePanel(tabMessages);
      }
      else
      {
        ImGui.Text("No messages to display.");
      }
    }


    public void MessagePanel(List<ChatEntry> messages)
    {
      ImGui.BeginChild("Messages");

      var isChatAtBottom = ImGui.GetScrollY() == ImGui.GetScrollMaxY();

      foreach (var chatItem in messages)
      {
        if (chatItem.SenderName == StateRepository.GetPlayerName())
        {
          ImGui.TextColored(ORANGE_COLOR, chatItem.DateSent.ToShortTimeString() + " " + chatItem.SenderName + ": ");
          ImGui.SameLine();
          ImGui.TextWrapped(chatItem.Message);
        }
        else
        {
          ImGui.Spacing();
          ImGui.Text(chatItem.DateSent.ToShortTimeString() + " " + chatItem.SenderName + ": ");
          ImGui.SameLine();
          ImGui.TextWrapped(chatItem.Message);
        }
      }

      if (AutoScrollToBottom == true)
      {
        ImGui.SetScrollY(ImGui.GetScrollMaxY());
      }

      ImGui.EndChild();
    }

    public void DrawSettingsWindow()
    {
      if (!SettingsVisible)
      {
        return;
      }

      ImGui.SetNextWindowSize(new Vector2(400, 350), ImGuiCond.Always);
      if (ImGui.Begin("Chat Scanner Configuration", ref this.settingsVisible,
          ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
      {
        if (ImGui.Checkbox("Open On Login", ref this.configuration.OpenOnLogin))
        {
          this.configuration.Save();
        }

        if (ImGui.Checkbox("Preserve Message History on Logout", ref this.configuration.PreserveMessagesOnLogout))
        {
          this.configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Text("Channels to Log");
        ImGui.Separator();

        var standardEmotes = this.configuration.AllowedChannels.Contains(XivChatType.StandardEmote);
        var customEmotes = this.configuration.AllowedChannels.Contains(XivChatType.CustomEmote);
        var party = this.configuration.AllowedChannels.Contains(XivChatType.Party);
        var say = this.configuration.AllowedChannels.Contains(XivChatType.Say);
        var whispers = this.configuration.AllowedChannels.Contains(XivChatType.TellOutgoing) && this.configuration.AllowedChannels.Contains(XivChatType.TellIncoming);

        if (ImGui.Checkbox("Say", ref say))
        {
          if (say)
          {
            this.configuration.AllowedChannels.Add(XivChatType.Say);
          }
          else
          {
            this.configuration.AllowedChannels.Remove(XivChatType.Say);
          }

          this.configuration.Save();
        }

        if (ImGui.Checkbox("Standard Emotes", ref standardEmotes))
        {
          if (standardEmotes)
          {
            this.configuration.AllowedChannels.Add(XivChatType.StandardEmote);
          }
          else
          {
            this.configuration.AllowedChannels.Remove(XivChatType.StandardEmote);
          }

          this.configuration.Save();
        }

        if (ImGui.Checkbox("Custom Emotes", ref customEmotes))
        {
          if (customEmotes)
          {
            this.configuration.AllowedChannels.Add(XivChatType.CustomEmote);
          }
          else
          {
            this.configuration.AllowedChannels.Remove(XivChatType.CustomEmote);
          }

          this.configuration.Save();
        }

        if (ImGui.Checkbox("Whispers", ref whispers))
        {
          if (whispers)
          {
            this.configuration.AllowedChannels.Add(XivChatType.TellIncoming);
            this.configuration.AllowedChannels.Add(XivChatType.TellOutgoing);
          }
          else
          {
            this.configuration.AllowedChannels.Remove(XivChatType.TellIncoming);
            this.configuration.AllowedChannels.Remove(XivChatType.TellOutgoing);
          }

          this.configuration.Save();
        }

        if (ImGui.Checkbox("Party", ref party))
        {
          if (party)
          {
            this.configuration.AllowedChannels.Add(XivChatType.Party);
          }
          else
          {
            this.configuration.AllowedChannels.Remove(XivChatType.Party);
          }

          this.configuration.Save();
        }

        ImGui.Separator();

        if (ImGui.Checkbox("Enable Debug Logging", ref this.configuration.DebugLogging))
        {
          this.configuration.Save();
        }

        if (configuration.DebugLogging)
        {

          if (ImGui.Checkbox("Track target tab creation", ref this.configuration.DebugLoggingCreatingTab))
          {
            this.configuration.Save();
          }
          // if (ImGui.Checkbox("Track target changing", ref this.configuration.DebugLoggingTargetChanging))
          // {
          //   this.configuration.Save();
          // }
          if (ImGui.Checkbox("Log Messages", ref this.configuration.DebugLoggingMessages))
          {
            this.configuration.Save();
          }

          if (configuration.DebugLoggingMessages)
          {
            ImGui.Text("--");
            ImGui.SameLine();
            if (ImGui.Checkbox("Log Message Payloads", ref this.configuration.DebugLoggingMessagePayloads))
            {
              this.configuration.Save();
            }

            ImGui.Text("--");
            ImGui.SameLine();
            if (ImGui.Checkbox("Log Message Contents", ref this.configuration.DebugLoggingMessageContents))
            {
              this.configuration.Save();
            }
          }
        }
      }
      ImGui.End();
    }
  }
}
