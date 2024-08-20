using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
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
    internal static string Name => _clientState.LocalPlayer?.Name.TextValue ?? "";

    /// <summary>
    /// Returns the currently logged in players world.
    /// </summary>
    internal static string World => _clientState.LocalPlayer?.HomeWorld.GameData?.Name ?? "";

    internal static PlayerIdentifier? GetPlayerIdentifier() {
        if (_clientState.LocalPlayer != null)
        {
            return new PlayerIdentifier(Name, World);
        }

        return null;
    }
}
