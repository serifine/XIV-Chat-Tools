using Dalamud.Game.Text;
using System;

namespace XIVChatTools.DB.Models;

public class Message
{
    public int Id { get; set; }

    public Player? OwningPlayer { get; set; }
    public required string OwningPlayerName { get; set; }
    public required string OwningPlayerWorld { get; set; }

    public required string SenderName { get; set; }
    public required string SenderWorld { get; set; }
    public required DateTime Timestamp { get; set; }
    public required XivChatType ChatType { get; set; }
    public required string MessageContents { get; set; }
}
