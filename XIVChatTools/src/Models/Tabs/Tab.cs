using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace XIVChatTools.Models.Tabs;

internal class Tab
{
    internal readonly Guid TabId;

    private string _title { get; set; }

    internal bool ShouldCloseNextFrame { get; private set; }
    internal bool IsPoppedOut { get; private set; }
    internal string Title { get => $"{_title}###{TabId.ToString()}"; set => _title = value; }

    internal Tab(string title = "")
    {
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
}
