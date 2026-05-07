using System;
using Dalamud.Game.Text;

namespace ChatTools.Models;

public class ChatEntry
{
  public DateTime DateSent = DateTime.Now;
  public XivChatType ChatType { get; set; }
  public int Timestamp { get; set; }
  public required string OwnerId { get; set; }
  public required string SenderName { get; set; }
  public required string Message { get; set; }
}
