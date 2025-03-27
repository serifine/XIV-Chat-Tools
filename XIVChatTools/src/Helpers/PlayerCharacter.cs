using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using XIVChatTools.Database;
using XIVChatTools.Database.Models;
using XIVChatTools.Models;

namespace XIVChatTools.Helpers;

/// <summary>
/// Helper class for working with the currently logged in character.
/// </summary>
internal static class PlayerCharacter
{
    private static IClientState _clientState = Plugin.ClientState;

    /// <summary>
    /// Returns the currently logged in players name.
    /// </summary>
    internal static string Name = "";

    /// <summary>
    /// Returns the currently logged in players world.
    /// </summary>
    internal static string World = "";

    internal async static void UpdatePlayerCharacter()
    {
        await Plugin.Framework.RunOnTick(() =>
        {
            Name = _clientState.LocalPlayer?.Name.TextValue ?? "";
            World = _clientState.LocalPlayer?.HomeWorld.Value.Name.ToString() ?? "";
        });
    }

    internal static PlayerIdentifier GetPlayerIdentifier()
    {
        return new PlayerIdentifier(Name, World);
    }
}
