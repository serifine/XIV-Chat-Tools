using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Game.Text;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ChatScanner
{
    public class ChannelType
    {
        public string Name { get; set; }
        public XivChatType ChatType { get; set; }
    }

    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool DebugLogging = false;
        public bool DebugLoggingMessages = true;
        public bool DebugLoggingMessageContents = true;
        public bool DebugLoggingMessagePayloads = true;
        public bool DebugLoggingCreatingTab = true;
        public bool DebugLoggingTargetChanging = true;
        public bool DebugLoggingMessageAsJson = false;

        public bool OpenOnLogin = false;

        public bool MessageLog_PreserveOnLogout = true;
        public bool MessageLog_ArchiveLogs = false;
        public bool MessageLog_DeleteOldMessages = true;
        public int MessageLog_DaysToKeepOldMessages = 7;
        public string MessageLog_FilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\XIVLauncher\\pluginConfigs\\ChatScanner";
        public string MessageLog_FileName = "ChatLogs.json";

        public List<XivChatType> ActiveChannels { get; set; } = new List<XivChatType>() {
            XivChatType.StandardEmote,
            XivChatType.CustomEmote,
            XivChatType.Party,
            XivChatType.Say,
            XivChatType.TellIncoming,
            XivChatType.TellOutgoing,
            XivChatType.Yell,
        };

        [NonSerialized]
        public List<ChannelType> AllChannels = new List<ChannelType>() {
            new ChannelType { Name = "Alliance",                  ChatType = XivChatType.Alliance         },
            new ChannelType { Name = "Cross World LinkShell 1",   ChatType = XivChatType.CrossLinkShell1  },
            new ChannelType { Name = "Cross World LinkShell 2",   ChatType = XivChatType.CrossLinkShell2  },
            new ChannelType { Name = "Cross World LinkShell 3",   ChatType = XivChatType.CrossLinkShell3  },
            new ChannelType { Name = "Cross World LinkShell 4",   ChatType = XivChatType.CrossLinkShell4  },
            new ChannelType { Name = "Cross World LinkShell 5",   ChatType = XivChatType.CrossLinkShell5  },
            new ChannelType { Name = "Cross World LinkShell 6",   ChatType = XivChatType.CrossLinkShell6  },
            new ChannelType { Name = "Cross World LinkShell 7",   ChatType = XivChatType.CrossLinkShell7  },
            new ChannelType { Name = "Cross World LinkShell 8",   ChatType = XivChatType.CrossLinkShell8  },
            new ChannelType { Name = "CrossParty",                ChatType = XivChatType.CrossParty       },
            new ChannelType { Name = "LinkShell 1",               ChatType = XivChatType.Ls1              },
            new ChannelType { Name = "LinkShell 2",               ChatType = XivChatType.Ls2              },
            new ChannelType { Name = "LinkShell 3",               ChatType = XivChatType.Ls3              },
            new ChannelType { Name = "LinkShell 4",               ChatType = XivChatType.Ls4              },
            new ChannelType { Name = "LinkShell 5",               ChatType = XivChatType.Ls5              },
            new ChannelType { Name = "LinkShell 6",               ChatType = XivChatType.Ls6              },
            new ChannelType { Name = "LinkShell 7",               ChatType = XivChatType.Ls7              },
            new ChannelType { Name = "LinkShell 8",               ChatType = XivChatType.Ls8              },
            new ChannelType { Name = "FreeCompany",               ChatType = XivChatType.FreeCompany      },
            new ChannelType { Name = "Shout",                     ChatType = XivChatType.Shout            },
            new ChannelType { Name = "Yell",                      ChatType = XivChatType.Yell             },
            new ChannelType { Name = "StandardEmote",             ChatType = XivChatType.StandardEmote    },
            new ChannelType { Name = "CustomEmote",               ChatType = XivChatType.CustomEmote      },
            new ChannelType { Name = "Party",                     ChatType = XivChatType.Party            },
            new ChannelType { Name = "Say",                       ChatType = XivChatType.Say              },
            new ChannelType { Name = "TellIncoming",              ChatType = XivChatType.TellIncoming     },
            new ChannelType { Name = "TellOutgoing",              ChatType = XivChatType.TellOutgoing     },
            new ChannelType { Name = "NoviceNetwork",             ChatType = XivChatType.NoviceNetwork    },
            new ChannelType { Name = "PvPTeam",                   ChatType = XivChatType.PvPTeam          }
        };
        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
