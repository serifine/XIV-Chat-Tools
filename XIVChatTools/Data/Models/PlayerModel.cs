using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XIVChatTools.Database.Models;

[Table("Players")]
[PrimaryKey(nameof(Name), nameof(World))]
public class Player
{
    public required string Name { get; set; }
    public required string World { get; set; }

    public ICollection<Message> OwnedMessages { get; set; } = new List<Message>();
}
