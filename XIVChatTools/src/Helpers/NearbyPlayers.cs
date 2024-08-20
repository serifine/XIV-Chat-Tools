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
public static class NearbyPlayers
{
    private static IObjectTable ObjectTable = Plugin.ObjectTable;

    /// <summary>
    /// Returns the a list of nearby players.
    /// </summary>
    public static List<IPlayerCharacter> GetNearbyPlayers()
    {
        return ObjectTable
          .Where(t => t.Name.TextValue != Helpers.PlayerCharacter.Name && t.ObjectKind == ObjectKind.Player)
          .Cast<IPlayerCharacter>()
          .OrderBy(t => t.Name.TextValue)
          .ToList();
    }

    /// <summary>
    /// Searches for a specific player from the object table.
    /// </summary>
    public static IPlayerCharacter? SearchForPlayer(string playerName, string? world)
    {
        var results = ObjectTable
          .Where(t => t.ObjectKind == ObjectKind.Player && t.Name.TextValue != playerName)
          .Cast<IPlayerCharacter>();
          
        if (world != null)
        {
            results = results.Where(t => t.HomeWorld.GameData != null && t.HomeWorld.GameData.Name == world);
        }

        return results.FirstOrDefault();
    }
}






