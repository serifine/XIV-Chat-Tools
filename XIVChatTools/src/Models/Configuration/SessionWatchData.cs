using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Game.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace XIVChatTools.Models.Configuration;

public class SessionWatchData
{
    public string GlobalWatchers = "";
    public string CharacterWatchers = "";

    public List<string> AllWatchers = new List<string>();

    public void UpdateGlobalWatchers(string watchers)
    {
        GlobalWatchers = watchers;

        RefreshAllWatchers();
    }

    public void UpdateCharacterWatchers(string watchers)
    {
        CharacterWatchers = watchers;
        
        RefreshAllWatchers();
    }

    internal void RefreshAllWatchers()
    {
        List<string> allWatchers = new List<string>();

        if (GlobalWatchers != "")
        {
            allWatchers.AddRange(GetWatchersFromString(GlobalWatchers));
        }

        if (CharacterWatchers != "")
        {
            allWatchers.AddRange(GetWatchersFromString(CharacterWatchers));
        }

        AllWatchers = allWatchers.Distinct().ToList();
    }

    private IEnumerable<string> GetWatchersFromString(string watchers)
    {
        return watchers.ToLower().Split(",").Select(s => s.Trim());
    }
}
