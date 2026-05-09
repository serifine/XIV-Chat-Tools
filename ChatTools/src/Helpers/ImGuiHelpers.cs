using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Utility;
using ChatTools;
using ChatTools.IO;

namespace ChatTools.Helpers;

public class ImGuiHelpers
{
    public static void DrawIcon(BitmapFontIcon icon)
    {
        var gfdEntry = TextureFileHelpers.GetGfdIconEntry(icon);
        var iconTexture = Plugin.TextureProvider.GetFromGame("common/font/fonticon_ps5.tex").GetWrapOrDefault();

        if (!gfdEntry.HasValue || iconTexture == null)
        {
            ImGui.Text("gfd entry or icon texture are null");
            return;
        }

        var fontSize = ImGui.GetFontSize();
        var textureSize = new Vector2(iconTexture.Width, iconTexture.Height);
        var iconSize = new Vector2(fontSize, fontSize) * Dalamud.Interface.Utility.ImGuiHelpers.GlobalScale;

        var uv0 = new Vector2(gfdEntry.Value.Left, gfdEntry.Value.Top + 170) * 2 / textureSize;
        var uv1 = new Vector2(gfdEntry.Value.Left + gfdEntry.Value.Width, gfdEntry.Value.Top + gfdEntry.Value.Height + 170) * 2 / textureSize;

        ImGui.Image(iconTexture.Handle, iconSize, uv0, uv1);
    }

    public static void DrawLabel(string label, float fontSize = 0.8f)
    {
        ImGui.SetWindowFontScale(fontSize);
        ImGui.Text(label);
        ImGui.SetWindowFontScale(1f);
    }
}