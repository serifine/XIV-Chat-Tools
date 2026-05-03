using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace XIVChatTools.UI.Components;

class FullWidthContainer : IDisposable
{
    private readonly float _originalCursorPosX;

    public FullWidthContainer(string id, float height = 24, float innerMargin = 4, float outerMargin = 4)
    {
        float boxWidth = ImGui.GetContentRegionAvail().X - outerMargin * 2;

        // Simulate top/left margin by offsetting the cursor
        var cursor = ImGui.GetCursorPos();
        ImGui.SetCursorPos(new Vector2(cursor.X + outerMargin, cursor.Y + outerMargin));

        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0, 0, 0, 1));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(innerMargin, innerMargin));

        ImGui.BeginChildFrame(ImGui.GetID(id), new Vector2(boxWidth, height + innerMargin * 2), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        
        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
    }

    public void Dispose() => ImGui.EndChildFrame();
}