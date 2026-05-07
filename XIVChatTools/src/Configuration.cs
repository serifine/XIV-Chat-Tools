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
    public int Version { get; set; } = 1;

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

    public Dictionary<ChatToolsColorCategory, Vector4> CustomChatColors = ColorConfigurations.DefaultColors;

    public Vector4 CharacterNameColor = ColorConfigurations.DefaultColors[ChatToolsColorCategory.CharacterName];
    public Vector4 SayColor = ColorConfigurations.DefaultColors[ChatToolsColorCategory.Say];
    public Vector4 EmoteColor = ColorConfigurations.DefaultColors[ChatToolsColorCategory.Emote];
    public Vector4 PartyColor = ColorConfigurations.DefaultColors[ChatToolsColorCategory.Party];
    public Vector4 TellColor = ColorConfigurations.DefaultColors[ChatToolsColorCategory.Tell];

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
        ColorConfigurations.SetCustomColors(this);
    }

    public void Save()
    {
        if (this._pluginInterface == null)
        {
            throw new InvalidOperationException("Plugin interface not set.");
        }

        ColorConfigurations.SetCustomColors(this);
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

    public void ReloadWatcherData()
    {
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

public enum ChatToolsColorCategory
{
    Watch,
    CharacterName,
    Emote,
    Party,
    Say,
    Tell,
    Yell,
}

internal static class ColorConfigurations
{
    internal static readonly Dictionary<ChatToolsColorCategory, Vector4> DefaultColors = new Dictionary<ChatToolsColorCategory, Vector4>()
    {
        { ChatToolsColorCategory.Watch, new Vector4(1.0f, 1.0f, 0.0f, 1.0f) },
        { ChatToolsColorCategory.CharacterName, new Vector4(255, 255, 255, 255) },
        { ChatToolsColorCategory.Say, new Vector4(255, 255, 255, 255) },
        { ChatToolsColorCategory.Emote, new Vector4(0.950f, 0.500f, 0f, 1f) },
        { ChatToolsColorCategory.Party, new Vector4(239, 122, 13, 255) },
        { ChatToolsColorCategory.Tell, new Vector4(239, 122, 13, 255) },
        { ChatToolsColorCategory.Yell, new Vector4(255, 255, 255, 255) },
    };

    internal static readonly Dictionary<ChatToolsColorCategory, Vector4> Colors = new Dictionary<ChatToolsColorCategory, Vector4>();

    internal static void SetCustomColors(Configuration config)
    {
        foreach (var color in config.CustomChatColors)
        {
            Colors[color.Key] = color.Value;
        }
    }

    internal static Vector4 GetColor(ChatToolsColorCategory category)
    {
        if (Colors.TryGetValue(category, out var color))
        {
            return color;
        }

        return DefaultColors[category];
    }
}
