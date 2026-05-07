using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace ChatTools.Models;

internal class PlayerIdentifier
{
    internal string Name { get; private set; }
    internal string World { get; private set; }

    internal PlayerIdentifier(IPlayerCharacter player)
    {
        if (!player.HomeWorld.IsValid)
            Plugin.Logger.Error($"Player {player.Name.TextValue} has no home world.");
        
        Name = player.Name.TextValue;
        World = player.HomeWorld.ValueNullable?.Name.ToString() ?? "Unknown World";
    }

    internal PlayerIdentifier(PlayerPayload player)
    {
        if (!player.World.IsValid)
            Plugin.Logger.Error($"Player {player.PlayerName} has no home world.");

        Name = player.PlayerName;
        World = player.World.ValueNullable?.Name.ToString() ?? "Unknown World";
    }

    internal PlayerIdentifier(string name, string world)
    {
        Name = name;
        World = world;
    }

    public bool Equals(PlayerIdentifier other)
    {
        return Name == other.Name && World == other.World;
    }
}
