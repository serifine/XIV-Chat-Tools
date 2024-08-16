using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace XIVChatTools.Models.Tabs;

internal class Tab
{
    internal readonly Guid TabId;

    internal bool ShouldCloseNextFrame { get; private set; }
    internal bool IsPoppedOut { get; private set; }
    internal string Title;

    internal Tab(string title = "")
    {
        TabId = Guid.NewGuid();
        Title = title;
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
