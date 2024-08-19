using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Microsoft.EntityFrameworkCore;

namespace XIVChatTools.Database.Models;

[Table("Players")]
[PrimaryKey(nameof(Name), nameof(World))]
public class Player
{
    public required string Name { get; set; }
    public required string World { get; set; }

    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<Message> OwnedMessages { get; set; } = new List<Message>();
}
