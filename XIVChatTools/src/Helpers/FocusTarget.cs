using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using XIVChatTools.Models;

namespace XIVChatTools.Helpers;

/// <summary>
/// Helper class for working with the TargetManager.
/// </summary>
internal static class FocusTarget
{
    private static ITargetManager TargetManager = Plugin.TargetManager;

    /// <summary>
    /// Returns the players current target or mouseover target if it is a player.
    /// </summary>
    internal static PlayerIdentifier? GetTargetedOrHoveredPlayer()
    {
        IGameObject? focusTarget = TargetManager.Target;

        if (focusTarget == null || focusTarget.ObjectKind != ObjectKind.Player)
        {
            focusTarget = TargetManager.MouseOverTarget;
        }

        if (focusTarget == null || focusTarget.ObjectKind != ObjectKind.Player)
        {
            return null;
        }

        return new PlayerIdentifier((IPlayerCharacter)focusTarget);
    }
}
