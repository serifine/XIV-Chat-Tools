using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Game.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

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

    public string MessageLog_FilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\XIVLauncher\\pluginConfigs\\ChatTools";
    public string MessageLog_FileName = "ChatLogs.json";
    public string MessageLog_Watchers = "";

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


    [NonSerialized] public List<ChannelType> AllChannels = Constants.AllChannels;

    //
    // the below exist just to make saving less cumbersome
    //

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        if (this.pluginInterface == null)
        {
            throw new InvalidOperationException("Plugin interface not set.");
        }

        this.pluginInterface.SavePluginConfig(this);
    }
}
