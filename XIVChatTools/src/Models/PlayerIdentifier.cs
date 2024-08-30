using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace XIVChatTools.Models;

internal class PlayerIdentifier
{
    internal string Name { get; private set; }
    internal string World { get; private set; }

    internal PlayerIdentifier(IPlayerCharacter player)
    {
        if (player.HomeWorld.GameData == null)
        {
            throw new InvalidOperationException("Player's HomeWorld is null");
        }

        Name = player.Name.TextValue;
        World = player.HomeWorld.GameData.Name;
    }

    internal PlayerIdentifier(PlayerPayload player)
    {
        Name = player.PlayerName;
        World = player.World.Name;
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
