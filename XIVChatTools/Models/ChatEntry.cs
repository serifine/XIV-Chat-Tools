using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text;

namespace XIVChatTools.Models;

public class ChatEntry
{
  public DateTime DateSent = DateTime.Now;
  public XivChatType ChatType { get; set; }
  public int Timestamp { get; set; }
  public required string OwnerId { get; set; }
  public required string SenderName { get; set; }
  public required string Message { get; set; }
}
