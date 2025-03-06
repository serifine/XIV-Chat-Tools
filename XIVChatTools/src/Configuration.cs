using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Game.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
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
    public string MessageDb_FilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\XIVLauncher\\pluginConfigs\\ChatTools";

    public string MessageLog_FilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\XIVLauncher\\pluginConfigs\\ChatTools";
    public string MessageLog_FileName = "ChatLogs.json";
    public string MessageLog_GlobalWatchers = "";
    public List<CharacterWatcher> MessageLog_CharacterWatchers = new List<CharacterWatcher>();

    public bool MessageLog_PreserveOnLogout = true;
    public bool MessageLog_DeleteOldMessages = true;
    public int MessageLog_DaysToKeepOldMessages = 7;

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
    private IDalamudPluginInterface? pluginInterface;

    [NonSerialized]
    internal SessionWatchData Session_WatchData = new SessionWatchData();

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;

        if (!System.IO.Directory.Exists(MessageDb_FilePath))
        {
            System.IO.Directory.CreateDirectory(MessageDb_FilePath);
        }

        ReloadWatcherData();
    }

    public void Save()
    {
        if (this.pluginInterface == null)
        {
            throw new InvalidOperationException("Plugin interface not set.");
        }

        this.pluginInterface.SavePluginConfig(this);
    }

    public void OnLoginUpdates()
    {
        ReloadWatcherData();
    }

    public void UpdateGlobalWatchers(string watchers)
    {
        MessageLog_GlobalWatchers = watchers;
        Session_WatchData.UpdateGlobalWatchers(watchers);

        Save();
    }

    public void UpdateCharacterWatchers(string watchers)
    {
        string CharacterName = Helpers.PlayerCharacter.Name;
        string WorldName = Helpers.PlayerCharacter.World;

        var characterWatcher = MessageLog_CharacterWatchers.FirstOrDefault(w => w.Character == CharacterName && w.World == WorldName);

        if (characterWatcher == null)
        {
            MessageLog_CharacterWatchers.Add(new CharacterWatcher(CharacterName, WorldName, watchers));
        }
        else
        {
            characterWatcher.Watchers = watchers;
        }

        Session_WatchData.UpdateCharacterWatchers(watchers);

        Save();
    }

    public void UpdateSessionWatchers(string watchers)
    {
        Session_WatchData.UpdateSessionWatchers(watchers);
    }

    public void ReloadWatcherData() {
        string CharacterName = Helpers.PlayerCharacter.Name;
        string WorldName = Helpers.PlayerCharacter.World;
        Session_WatchData = new SessionWatchData();
        
        CharacterWatcher? characterWatcher = MessageLog_CharacterWatchers.FirstOrDefault(w => w.Character == Helpers.PlayerCharacter.Name && w.World == Helpers.PlayerCharacter.World);
    
        if (MessageLog_GlobalWatchers != "")
        {
            Session_WatchData.UpdateGlobalWatchers(MessageLog_GlobalWatchers);
        }

        if (characterWatcher != null)
        {
            Session_WatchData.UpdateCharacterWatchers(characterWatcher.Watchers);
        }
    }
}
