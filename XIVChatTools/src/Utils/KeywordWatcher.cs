
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace XIVChatTools;

/// <summary>
/// Helper class for checking various watchers and sending notifications on matches.
/// </summary>
internal class KeywordWatcher
{
    private readonly Plugin _plugin;

    private Configuration Configuration => _plugin.Configuration;
    private IPluginLog Logger => Plugin.Logger;

    internal KeywordWatcher(Plugin plugin)
    {
        _plugin = plugin;
    }

    /// <summary>
    /// Checks if the message contains any watched terms and plays a notification if it does.
    /// </summary>
    internal void HandleMessage(List<IMessagePart> messageParts)
    {
        var message = string.Join("", messageParts.Select(part => part.ToString()));

        if (IsWatchedTerm(message))
        {
            PlayNotification();
        }
    }

    internal bool IsWatchedTerm(string message)
    {
        var watchers = Configuration.SessionWatchData.AllWatchers;
        
        if (watchers.Count == 0) return false;

        return watchers.Any(watcher => message.ToLower().Contains(watcher));
    }

    private void PlayNotification()
    {
        try
        {
            UIGlobals.PlayChatSoundEffect(2);
        }
        catch (Exception ex)
        {
            Logger.Debug("Error playing sound via Dalamud.");
            Logger.Debug(ex.Message);
        }
    }
}
