
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
    internal void HandleMessage(string message)
    {
        if (ContainsWatchedTerm(message))
        {
            PlayNotification();
        }
    }

    private bool ContainsWatchedTerm(string message)
    {
        var watchers = Configuration.Session_WatchData.AllWatchers;
        
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
