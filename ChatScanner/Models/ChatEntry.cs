using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text;

namespace ChatScanner.Models
{
    public class ChatEntry
    {
      public DateTime DateSent = DateTime.Now;
      public XivChatType ChatType;
      public int Timestamp;
      public string OwnerId;
      public string SenderName;
      public string Message;
    }
}
