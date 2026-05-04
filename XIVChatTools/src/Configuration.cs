using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using XIVChatTools.Models.Configuration;

namespace XIVChatTools;

public class ChannelType
{
    public required string Name { get; set; }
    public Vector4 Color { get; set; }
    public XivChatType ChatType { get; set; }
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool OpenOnLogin = false;

    public bool SplitDateAndNames = true;

    #region Chat Log Settings
    public string MessageLogGlobalWatchers = "";
    public List<CharacterWatcher> MessageLogCharacterWatchers = new List<CharacterWatcher>();

    public bool MessageLogPreserveOnLogout = true;
    public bool MessageLogDeleteOldMessages = true;
    public int MessageLogDaysToKeepOldMessages = 7;

    #endregion

    #region Channel and Chat Settings

    public bool DisableCustomChatColors = false;
    public Vector4 CharacterNameColor = new Vector4(255, 255, 255, 255);
    public Vector4 NormalChatColor = new Vector4(255, 255, 255, 255);
    public Vector4 EmoteColor = new Vector4(0.950f, 0.500f, 0f, 1f);
    public Vector4 PartyColor = new Vector4(239, 122, 13, 255);
    public Vector4 TellColor = new Vector4(239, 122, 13, 255);

    public List<XivChatType> ActiveChannels { get; set; } = new List<XivChatType>() {
            XivChatType.StandardEmote,
            XivChatType.CustomEmote,
            XivChatType.Party,
            XivChatType.Say,
            XivChatType.TellIncoming,
            XivChatType.TellOutgoing,
            XivChatType.Yell,
        };

    #endregion

    #region Debug Logging Settings

    public bool DebugLogging = false;

    #endregion

    //
    // the below exist just to make saving less cumbersome
    //

    [NonSerialized]
    private IDalamudPluginInterface? _pluginInterface;

    [NonSerialized]
    internal SessionWatchData SessionWatchData = new SessionWatchData();

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this._pluginInterface = pluginInterface;

        ReloadWatcherData();
    }

    public void Save()
    {
        if (this._pluginInterface == null)
        {
            throw new InvalidOperationException("Plugin interface not set.");
        }

        this._pluginInterface.SavePluginConfig(this);
    }

    public void OnLoginUpdates()
    {
        ReloadWatcherData();
    }

    public void UpdateGlobalWatchers(string watchers)
    {
        MessageLogGlobalWatchers = watchers;
        SessionWatchData.UpdateGlobalWatchers(watchers);

        Save();
    }

    public void UpdateCharacterWatchers(string watchers)
    {
        string characterName = Helpers.PlayerCharacter.Name;
        string worldName = Helpers.PlayerCharacter.World;

        var characterWatcher = MessageLogCharacterWatchers.FirstOrDefault(w => w.Character == characterName && w.World == worldName);

        if (characterWatcher == null)
        {
            MessageLogCharacterWatchers.Add(new CharacterWatcher(characterName, worldName, watchers));
        }
        else
        {
            characterWatcher.Watchers = watchers;
        }

        SessionWatchData.UpdateCharacterWatchers(watchers);

        Save();
    }

    public void UpdateSessionWatchers(string watchers)
    {
        SessionWatchData.UpdateSessionWatchers(watchers);
    }

    public void ReloadWatcherData() {
        string characterName = Helpers.PlayerCharacter.Name;
        string worldName = Helpers.PlayerCharacter.World;
        SessionWatchData = new SessionWatchData();
        
        CharacterWatcher? characterWatcher = MessageLogCharacterWatchers.FirstOrDefault(w => w.Character == Helpers.PlayerCharacter.Name && w.World == Helpers.PlayerCharacter.World);
    
        if (MessageLogGlobalWatchers != "")
        {
            SessionWatchData.UpdateGlobalWatchers(MessageLogGlobalWatchers);
        }

        if (characterWatcher != null)
        {
            SessionWatchData.UpdateCharacterWatchers(characterWatcher.Watchers);
        }
    }
}
