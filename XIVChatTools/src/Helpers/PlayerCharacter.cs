using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

namespace XIVChatTools.Helpers;

/// <summary>
/// Helper class for working with the currently logged in character.
/// </summary>
public static class PlayerCharacter
{
    private static IClientState _clientState = Plugin.ClientState;

    /// <summary>
    /// Returns the currently logged in players name.
    /// </summary>
    public static string Name => _clientState.LocalPlayer?.Name.TextValue ?? "";
}
