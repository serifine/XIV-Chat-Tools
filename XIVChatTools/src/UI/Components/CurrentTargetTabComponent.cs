

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XIVChatTools.Database.Models;
using XIVChatTools.Models;
using XIVChatTools.Models.Tabs;
using XIVChatTools.Services;

namespace XIVChatTools.UI.Components;


internal class FocusTargetTabComponent : IDisposable
{
    private readonly Plugin _plugin;
    private readonly MessagePanel _messagePanel;

    private List<Message> messages = new List<Message>();
    private MessageService _messageService => _plugin.MessageService;
    private PlayerIdentifier? currentFocusedTarget = null;

    private float Scale => ImGui.GetIO().FontGlobalScale;

    public FocusTargetTabComponent(Plugin plugin)
    {
        _plugin = plugin;
        _messagePanel = new(plugin);
        _messageService.MessageAdded += OnMessageAdded;
    }

    public void Dispose()
    {
        _messageService.MessageAdded -= OnMessageAdded;
    }

    private void OnMessageAdded(PlayerIdentifier sender, Message message)
    {
        if (currentFocusedTarget != null && currentFocusedTarget.Equals(sender))
        {
            messages.Add(message);
        }
    }

    internal void PreDraw()
    {
        var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

        if (focusTarget == null)
        {
            currentFocusedTarget = null;
            messages = new List<Message>();
            return;
        }

        if (currentFocusedTarget == null || !focusTarget.Equals(currentFocusedTarget))
        {
            currentFocusedTarget = focusTarget;
            messages = _messageService.GetMessagesForPlayer(focusTarget);
        }
    }

    internal void Draw()
    {
        if (ImGui.BeginTabItem("Current Target"))
        {
            if (currentFocusedTarget == null)
            {
                ImGui.Text("No target hovered or selected.");
            }
            else if (messages != null && messages.Count > 0)
            {
                _messagePanel.Draw(messages);
            }
            else
            {
                ImGui.Text("No messages found for " + currentFocusedTarget.Name + ".");
            }

            ImGui.EndTabItem();
        }
    }
}
