using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using ChatTools.Models.Configuration;

namespace ChatTools;

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

    public Dictionary<ColorCategory, Vector4> CustomChatColors = ColorConfigurations.DefaultColors.ToDictionary();

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

        CharacterWatcher? characterWatcher = MessageLogCharacterWatchers.FirstOrDefault(w => w.Character == characterName && w.World == worldName);

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

public enum ColorCategory
{
    Watch,
    Emote,
    Party,
    Say,
    Tell,
    Yell,
}

/// <summary>
/// Handles the color configurations for Chat Tools, hoisting them into a static method that can be accessed anywhere in the plugin.
/// </summary>
internal static class ColorConfigurations
{
    internal static event Action? ColorsUpdated;

    internal static readonly Dictionary<ColorCategory, Vector4> DefaultColors = new Dictionary<ColorCategory, Vector4>()
    {
        { ColorCategory.Watch, new Vector4(0f, 0.88f, 1f, 1f) },
        { ColorCategory.Say, new Vector4(1f, 1f, 1f, 1f) },
        { ColorCategory.Emote, new Vector4(0.950f, 0.500f, 0f, 1f) },
        { ColorCategory.Party, new Vector4(0.937f, 0.478f, 0.051f, 1f) },
        { ColorCategory.Tell, new Vector4(0.937f, 0.478f, 0.051f, 1f) },
        { ColorCategory.Yell, new Vector4(1f, 1f, 1f, 1f) },
    };

    private static readonly Dictionary<ColorCategory, Vector4> Colors = new Dictionary<ColorCategory, Vector4>();

    internal static void SetCustomColors(Configuration config)
    {
        foreach (var color in config.CustomChatColors)
        {
            Colors[color.Key] = color.Value;
        }

        ColorsUpdated?.Invoke();
    }

    internal static Vector4 GetColor(ColorCategory category)
    {
        if (Colors.TryGetValue(category, out var color))
        {
            return color;
        }

        return DefaultColors[category];
    }
}
