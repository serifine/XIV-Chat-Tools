using System.Collections.Generic;

namespace XIVChatTools.DB.Models;

public class Player
{
    public required string Name { get; set; }
    public required string World { get; set; }

    public ICollection<Message> OwnedMessages { get; set; } = new List<Message>();
}
