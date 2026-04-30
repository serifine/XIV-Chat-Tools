using System;

namespace XIVChatTools.Models.Tabs;

internal class Tab : IDisposable
{
    internal Plugin Plugin;
    internal readonly Guid TabId;

    internal bool ShouldCloseNextFrame { get; private set; }
    internal bool IsPoppedOut { get; private set; }
    internal string Title = "";

    internal Tab(Plugin plugin, string title = "")
    {
        Plugin = plugin;
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

    public virtual void Dispose()
    {
        
    }
}
