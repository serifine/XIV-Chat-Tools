using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using XIVChatTools.Models;

namespace XIVChatTools.Helpers;

/// <summary>
/// Helper class for working with the TargetManager.
/// </summary>
internal static class FocusTarget
{
    private static ITargetManager _targetManager = Plugin.TargetManager;

    /// <summary>
    /// Returns the players current target or mouseover target if it is a player.
    /// </summary>
    internal static PlayerIdentifier? GetTargetedOrHoveredPlayer()
    {
        IGameObject? focusTarget = _targetManager.Target;

        if (focusTarget is not { ObjectKind: ObjectKind.Pc })
        {
            focusTarget = _targetManager.MouseOverTarget;
        }

        if (focusTarget is not { ObjectKind: ObjectKind.Pc })
        {
            return null;
        }

        return new PlayerIdentifier((IPlayerCharacter)focusTarget);
    }
}
