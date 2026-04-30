using System;
using Dalamud.IoC;

namespace XIVChatTools.Services;

[PluginInterface]
public class PluginStateService : IDisposable
{
    private readonly Plugin _plugin;

    public PluginStateService(Plugin plugin)
    {
        _plugin = plugin;
    }

    public void Dispose()
    {

    }
}
