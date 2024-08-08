using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Game.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace XIVChatTools
{
    public class ChannelType
    {
        public required string Name { get; set; }
        public Vector4 Color { get; set; }
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

        public bool DisableCustomChatColors = false;
        public bool MessageLog_PreserveOnLogout = true;
        public bool MessageLog_ArchiveLogs = false;
        public bool MessageLog_DeleteOldMessages = true;
        public int MessageLog_DaysToKeepOldMessages = 7;
        public string MessageLog_FilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\XIVLauncher\\pluginConfigs\\XIVChatTools";
        public string MessageLog_FileName = "ChatLogs.json";
        public string MessageLog_Watchers = "";

        public bool SplitDateAndNames = true;

        public Vector4 CharacterNameColor = new Vector4(255, 255, 255, 255);
        public Vector4 NormalChatColor = new Vector4(255, 255, 255, 255);
        public Vector4 EmoteColor = new Vector4(0.950f, 0.500f, 0f, 1f);
        public Vector4 PartyColor = new Vector4(239, 122, 13, 255);
        public Vector4 TellColor = new Vector4(239, 122, 13, 255);

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
        };
        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            if (this.pluginInterface == null)
            {
                throw new InvalidOperationException("Plugin interface not set.");
            }

            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
