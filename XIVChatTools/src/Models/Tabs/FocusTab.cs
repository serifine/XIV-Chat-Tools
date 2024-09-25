using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using XIVChatTools.Database.Models;
using XIVChatTools.Services;

namespace XIVChatTools.Models.Tabs;

internal class FocusTab : Tab
{
    private List<PlayerIdentifier> _focusTargets;

    private MessageService _messageService => _plugin.MessageService;

    internal List<Message> messages = new List<Message>();

    internal FocusTab(Plugin plugin, PlayerIdentifier initialTarget, string title = "New Watcher") : base(plugin, title)
    {
        _focusTargets = new List<PlayerIdentifier>() { initialTarget };

        _messageService.MessageAdded += OnMessageAdded;

        UpdateMessagesFromDb();
    }

    public override void Dispose()
    {
        _messageService.MessageAdded -= OnMessageAdded;
    }

    private void UpdateMessagesFromDb()
    {
        this.messages = _messageService.GetMessagesForPlayers(_focusTargets);
    }

    internal void OnMessageAdded(PlayerIdentifier sender, Message message)
    {
        if (_focusTargets.Any(t => t.Equals(sender)))
        {
            messages.Add(message);
        }
    }

    internal List<PlayerIdentifier> GetFocusTargets()
    {
        return _focusTargets.ToList();
    }

    internal void AddFocusTarget(PlayerIdentifier target)
    {
        if (this._focusTargets.Any(t => t.Equals(target)) == false)
        {
            this._focusTargets.Add(target);
            UpdateMessagesFromDb();
        }
    }

    internal void RemoveFocusTarget(PlayerIdentifier target)
    {
        var selectedTarget = this._focusTargets.FirstOrDefault(t => t.Equals(target));

        if (selectedTarget != null)
        {
            this._focusTargets.Remove(selectedTarget);
            UpdateMessagesFromDb();
        }
    }

    internal bool IsPlayerAdded(PlayerIdentifier target)
    {
        return this._focusTargets.Any(t => t.Equals(target));
    }
}


