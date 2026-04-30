using System;
using Dalamud.Plugin.Services;
using XIVChatTools.Models;

namespace XIVChatTools.Helpers;

/// <summary>
/// Helper class for working with the currently logged-in character.
/// </summary>
internal static class PlayerCharacter
{
    private static IPlayerState PlayerState => Plugin.PlayerState;
    private static IPluginLog Logger => Plugin.Logger;

    /// <summary>
    /// Returns the currently logged-in players name.
    /// </summary>
    internal static string Name = "";

    /// <summary>
    /// Returns the currently logged-in players world.
    /// </summary>
    internal static string World = "";

    internal static async void UpdatePlayerCharacter()
    {
        try
        {
            await Plugin.Framework.RunOnTick(() =>
            {
                if (PlayerState.IsLoaded)
                {
                    Name = PlayerState.CharacterName;
                    World = PlayerState.HomeWorld.ValueNullable?.Name.ToString() ?? "";
                }
                else
                {
                    Name = "";
                    World = "";
                }
            });
        }
        catch (Exception error)
        {
            Logger.Error("Error Updating Character Information: " + error.Message);
        }
    }

    internal static PlayerIdentifier GetPlayerIdentifier()
    {
        return new PlayerIdentifier(Name, World);
    }
}