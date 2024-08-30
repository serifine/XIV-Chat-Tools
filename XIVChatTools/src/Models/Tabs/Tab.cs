using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace XIVChatTools.Models.Tabs;

internal class Tab : IDisposable
{
    internal Plugin _plugin;
    internal readonly Guid TabId;

    private string _title { get; set; }

    internal bool ShouldCloseNextFrame { get; private set; }
    internal bool IsPoppedOut { get; private set; }
    internal string Title { get => $"{_title}###{TabId.ToString()}"; set => _title = value; }

    internal Tab(Plugin plugin, string title = "")
    {
        _plugin = plugin;
        TabId = Guid.NewGuid();
        _title = title;
    }

    internal void Close()
    {
        if (IsPoppedOut)
        {
            IsPoppedOut = false;
        }
        else
        {
            ShouldCloseNextFrame = true;
        }
    }

    internal void PopoutTab()
    {
        IsPoppedOut = true;
    }

    public virtual void Dispose()
    {
        
    }
}
