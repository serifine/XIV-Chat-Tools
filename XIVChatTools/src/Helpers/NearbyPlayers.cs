using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

namespace XIVChatTools.Helpers;

/// <summary>
/// Helper class for working with NearbyPlayers.
/// </summary>
internal static class NearbyPlayers
{
    private static IObjectTable _objectTable = Plugin.ObjectTable;
    private static IPluginLog _logger = Plugin.Logger;

    /// <summary>
    /// Returns the a list of nearby players.
    /// </summary>
    internal static List<IPlayerCharacter> GetNearbyPlayers()
    {
        return _objectTable
          .Where(t => t.Name.TextValue != Helpers.PlayerCharacter.Name && t.ObjectKind == ObjectKind.Player)
          .Cast<IPlayerCharacter>()
          .OrderBy(t => t.Name.TextValue)
          .ToList();
    }

    /// <summary>
    /// Searches for a specific player from the object table.
    /// </summary>
    internal static IPlayerCharacter? SearchForPlayerByName(string playerName)
    {
        var results = _objectTable
          .Where(t => t.ObjectKind == ObjectKind.Player && t.Name.TextValue != playerName)
          .Cast<IPlayerCharacter>();

        return results.FirstOrDefault();
    }
}






