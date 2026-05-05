using System;
using Dalamud.Game.Text.SeStringHandling;

namespace XIVChatTools.IO;

internal static class TextureFileHelpers
{
    private static byte[]? CachedFile;

    public static GfdFileView.GfdEntry? GetGfdIconEntry(BitmapFontIcon icon)
    {
        GfdFileView.GfdEntry? gfdEntryResult = null;
        CachedFile ??= Plugin.DataManager.GetFile("common/font/gfdata.gfd")!.Data;

        bool getEntrySuccess = new GfdFileView(new ReadOnlySpan<byte>(CachedFile)).TryGetEntry((uint)icon, out var entry);

        if (getEntrySuccess)
        {
            gfdEntryResult = entry;
        }

        return gfdEntryResult;
    }
}
