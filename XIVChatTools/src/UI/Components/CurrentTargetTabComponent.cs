using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using XIVChatTools.DB.Models;
using XIVChatTools.Models;
using XIVChatTools.Services;


namespace XIVChatTools.UI.Components;

internal class FocusTargetTabComponent : IDisposable
{
    private readonly MessagePanel _messagePanel;
    private readonly MessageService _messageService;

    private PlayerIdentifier? _currentFocusedTarget;
    private List<Message> _messages = [];

    public FocusTargetTabComponent(Plugin plugin)
    {
        _messagePanel = new MessagePanel(plugin);
        _messageService = plugin.MessageService;
        _messageService.MessageAdded += OnMessageAdded;
    }

    public void Dispose()
    {
        _messageService.MessageAdded -= OnMessageAdded;
    }

    private void OnMessageAdded(PlayerIdentifier sender, Message message)
    {
        if (_currentFocusedTarget != null && _currentFocusedTarget.Equals(sender))
        {
            _messages.Add(message);
        }
    }

    internal void PreDraw()
    {
        var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

        if (focusTarget == null) {
            _currentFocusedTarget = null;
            _messages = [];
            return;
        }

        if (_currentFocusedTarget != null && focusTarget.Equals(_currentFocusedTarget)) return;
        
        _currentFocusedTarget = focusTarget;
        _messages = _messageService.GetMessagesForPlayer(focusTarget);
    }

    internal void Draw()
    {
        if (ImGui.BeginTabItem("Current Target"))
        {
            DrawContent();
            ImGui.EndTabItem();
        }
    }

    internal void DrawContent()
    {
        if (_currentFocusedTarget == null)
        {
            ImGui.Text("No target hovered or selected.");
        }
        else if (_messages.Count > 0)
        {
            _messagePanel.Draw(_messages);
        }
        else
        {
            ImGui.Text("No messages found for " + _currentFocusedTarget.Name + ".");
        }
    }
}
