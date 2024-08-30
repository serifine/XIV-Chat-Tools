

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dalamud.Game.Text;
using Microsoft.EntityFrameworkCore;

namespace XIVChatTools.Database.Models;

[Table("Messages")]
public class Message
{
    [Key]
    public int Id { get; set; }

    public required Player OwningPlayer { get; set; }
 
    public required string SenderName { get; set; }
    public required string SenderWorld { get; set; }
    public required DateTime Timestamp { get; set; }
    public required XivChatType ChatType { get; set; }
    public required string MessageContents { get; set; }
}
