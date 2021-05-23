using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using ChatScanner.Models;

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
      // This is our only draw handler attached to UIBuilder, so it needs to be
      // able to draw any windows we might have open.
      // Each method checks its own visibility/state to ensure it only draws when
      // it actually makes sense.
      // There are other ways to do this, but it is generally best to keep the number of
      // draw delegates as low as possible.

      DrawMainWindow();
      DrawSettingsWindow();
    }

    private bool autoScrollToBottom = false;
    public bool AutoScrollToBottom
    {
      get { return this.autoScrollToBottom; }
      set { this.autoScrollToBottom = value; }
    }

    // this is where you'd have to start mocking objects if you really want to match
    // but for simple UI creation purposes, just hardcoding values works
    private bool fakeConfigBool = true;
    private string comboCurrentValue = "Focus Target";

    private readonly Vector4 ORANGE_COLOR = new Vector4(0.950f, 0.500f, 0f, 1f);
    private readonly Vector4 LIGHT_ORANGE_COLOR = new Vector4(0.950f, 0.650f, 0f, 1f);

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
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - (190 * scale));
        if (ImGui.Button("Add Focus Target"))
        {
          StateRepository.AddFocusTabFromTarget();
        }

        // ImGui.Separator();

        if (ImGui.BeginTabBar("MainTabs", ImGuiTabBarFlags.Reorderable))
        {
          if (ImGui.BeginTabItem("Selected Target"))
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

            ImGui.EndTabItem();
          }

          if (ImGui.BeginTabItem("All Messages"))
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

            ImGui.EndTabItem();
          }

          // if (ImGui.BeginTabItem("Keyword Scan"))
          // {
          //   MessagePanel();
          //   ImGui.EndTabItem();
          // }

          foreach (var focusTab in StateRepository.GetFocusTabs())
          {
            if (ImGui.BeginTabItem(focusTab.Name, ref focusTab.Open, ImGuiTabItemFlags.None))
            {
              var focusNames = focusTab.GetFocusTargets();
              // var comboItems = new List<string>() { "Focus Target", "Saya Naeuri (you)" };
              // comboItems.AddRange(focusNames);

              if (ImGui.BeginTable("table1", 2, ImGuiTableFlags.NoHostExtendX))
              {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                foreach (var name in focusNames)
                {
                  ImGui.TableNextRow();

                  ImGui.TableSetColumnIndex(0);
                  // if (names.Count() > 0)
                  // {
                  ImGui.PushID("Button" + name);
                  if (ImGui.SmallButton("Remove"))
                  {
                    focusTab.RemoveFocusTarget(name);
                  }
                  // } else {
                  //   ImGui.Text("Cant Remove");
                  // }

                  ImGui.TableSetColumnIndex(1);
                  ImGui.Text(name);
                }
              }
              ImGui.EndTable();

              ImGui.SameLine(ImGui.GetContentRegionAvail().X - (300 * scale));

              // ImGui.BeginGroup();


              ImGui.SetNextItemWidth(200);
              // ImGui.Combo(" ", ref this.comboCurrentItem, comboItems.ToArray(), comboItems.Count());
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
                    // focusTab.AddFocusTarget(actor.Name);
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
                    focusTab.AddFocusTarget(focusTarget.Name);
                  }
                }
                else
                {
                  focusTab.AddFocusTarget(comboCurrentValue);
                }
                comboCurrentValue = "Focus Target";
              }
              // ImGui.EndGroup();

              ImGui.Separator();

              var tabMessages = StateRepository.GetMessagesByPlayerNames(focusTab.GetFocusTargets());

              if (tabMessages.Count() > 0)
              {
                MessagePanel(tabMessages);
              }
              else
              {
                ImGui.Text("No messages to display.");
              }

              ImGui.EndTabItem();
            }
          }

          StateRepository.RemoveClosedFocusTabs();

          ImGui.EndTabBar();
        }
      }
      ImGui.End();
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

      ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
      if (ImGui.Begin("A Wonderful Configuration Window", ref this.settingsVisible,
          ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
      {
        if (ImGui.Checkbox("Random Config Bool", ref this.fakeConfigBool))
        {
          // nothing to do in a fake ui!
        }
      }
      ImGui.End();
    }
  }
}
