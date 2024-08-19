

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XIVChatTools.Database.Models;

[Table("Messages")]
public class Message
{
    [Key]
    public int Id { get; set; }

    public required DateTime Timestamp { get; set; }
    public required string MessageContents { get; set; }

    public required Player OwningPlayer { get; set; }

    public required string SenderName { get; set; }
    public required string SenderWorld { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientWorld { get; set; }

}
