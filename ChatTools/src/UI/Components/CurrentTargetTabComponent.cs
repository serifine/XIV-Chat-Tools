using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using ChatTools.DB.Models;
using ChatTools.Models;
using ChatTools.Services;


namespace ChatTools.UI.Components;

internal class FocusTargetTabComponent : IDisposable
{
    private readonly MessagePanel _messagePanel;
    private readonly MessageService _messageService;
    private readonly Configuration _configuration;

    private PlayerIdentifier? _currentFocusedTarget;
    private List<Message> _messages = [];

    public FocusTargetTabComponent(Plugin plugin)
    {
        _messagePanel = new MessagePanel(plugin);
        _messageService = plugin.MessageService;
        _configuration = plugin.Configuration;
        _messageService.MessageAdded += OnMessageAdded;

        LoadAllMessages();
    }

    public void Dispose()
    {
        _messageService.MessageAdded -= OnMessageAdded;
    }

    private void OnMessageAdded(PlayerIdentifier sender, Message message)
    {
        if (_currentFocusedTarget != null && _currentFocusedTarget.Matches(sender))
        {
            _messages.Add(message);
        }
        else if (_currentFocusedTarget == null)
        {
            _messages.Add(message);
        }
    }

    internal void PreDraw()
    {
        var focusTarget = Helpers.FocusTarget.GetTargetedOrHoveredPlayer();

        if (focusTarget == null && _currentFocusedTarget != null)
        {
            _currentFocusedTarget = null;

            LoadAllMessages();

            return;
        }

        if ((_currentFocusedTarget == null && focusTarget != null) || (focusTarget != null && _currentFocusedTarget != null && !focusTarget.Matches(_currentFocusedTarget)))
        {
            _currentFocusedTarget = focusTarget;

            _messages = _messageService.GetMessagesForPlayer(focusTarget);
        }
    }

    internal void Draw()
    {
        DrawContent();
    }

    private void DrawContent()
    {
        if (_messages.Count > 0)
        {
            _messagePanel.Draw(_messages);
        }
        else if (_currentFocusedTarget != null)
        {
            ImGui.Text("No messages found for " + _currentFocusedTarget.Name + ".");
        }
        else
        {
            ImGui.Text("No messages to display.");
        }
    }

    private void LoadAllMessages()
    {
        if (_configuration.MessageLogShowAllMessagesInMainTab)
        {
            Plugin.Logger.Verbose("No focus target, showing all messages in main tab");
            _messages = _messageService.GetAllMessages();
        }
        else
        {
            Plugin.Logger.Verbose("No focus target, clearing messages");
            _messages = [];
        }
    }
}
