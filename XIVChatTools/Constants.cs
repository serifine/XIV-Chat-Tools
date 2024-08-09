
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Text;

namespace XIVChatTools;

public static class Constants
{
    public static readonly List<ChannelType> AllChannels = [
      new ChannelType { Name = "Alliance",                Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Alliance         },
    new ChannelType { Name = "Cross World LinkShell 1", Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CrossLinkShell1  },
    new ChannelType { Name = "Cross World LinkShell 2", Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CrossLinkShell2  },
    new ChannelType { Name = "Cross World LinkShell 3", Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CrossLinkShell3  },
    new ChannelType { Name = "Cross World LinkShell 4", Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CrossLinkShell4  },
    new ChannelType { Name = "Cross World LinkShell 5", Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CrossLinkShell5  },
    new ChannelType { Name = "Cross World LinkShell 6", Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CrossLinkShell6  },
    new ChannelType { Name = "Cross World LinkShell 7", Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CrossLinkShell7  },
    new ChannelType { Name = "Cross World LinkShell 8", Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CrossLinkShell8  },
    new ChannelType { Name = "CrossParty",              Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CrossParty       },
    new ChannelType { Name = "LinkShell 1",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Ls1              },
    new ChannelType { Name = "LinkShell 2",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Ls2              },
    new ChannelType { Name = "LinkShell 3",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Ls3              },
    new ChannelType { Name = "LinkShell 4",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Ls4              },
    new ChannelType { Name = "LinkShell 5",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Ls5              },
    new ChannelType { Name = "LinkShell 6",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Ls6              },
    new ChannelType { Name = "LinkShell 7",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Ls7              },
    new ChannelType { Name = "LinkShell 8",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Ls8              },
    new ChannelType { Name = "FreeCompany",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.FreeCompany      },
    new ChannelType { Name = "Shout",                   Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Shout            },
    new ChannelType { Name = "Yell",                    Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Yell             },
    new ChannelType { Name = "StandardEmote",           Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.StandardEmote    },
    new ChannelType { Name = "CustomEmote",             Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.CustomEmote      },
    new ChannelType { Name = "Party",                   Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Party            },
    new ChannelType { Name = "Say",                     Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.Say              },
    new ChannelType { Name = "TellIncoming",            Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.TellIncoming     },
    new ChannelType { Name = "TellOutgoing",            Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.TellOutgoing     },
    new ChannelType { Name = "NoviceNetwork",           Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.NoviceNetwork    },
    new ChannelType { Name = "PvPTeam",                 Color = new Vector4(0, 0, 0, 0),    ChatType = XivChatType.PvPTeam          }
    ];
}