using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChatTools.UI;

public partial class StyleManager(Plugin plugin) : IDisposable
{
    private bool _stylesApplied = false;
    private Configuration Configuration => plugin.Configuration;
    private Dictionary<ImGuiStyleVar, float> StyleVarFloats { get; set; } = new()
    {
        { ImGuiStyleVar.PopupRounding,      4 },
        { ImGuiStyleVar.PopupBorderSize,  1.0f },
        { ImGuiStyleVar.WindowBorderSize, 1.0f },
    };

    private Dictionary<ImGuiStyleVar, Vector2> StyleVarVectors { get; set; } = new()
    {
    };

    private Dictionary<ImGuiCol, Vector4> StyleColors { get; set; } = new()
    {
        { ImGuiCol.Text,                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f) },
        { ImGuiCol.TextDisabled,            new Vector4(0.5f, 0.5f, 0.5f, 1.0f) },
        { ImGuiCol.WindowBg,                new Vector4(0.025f, 0.025f, 0.025f, 1.0f) },
        { ImGuiCol.PopupBg,                 new Vector4(0.03f, 0.03f, 0.03f, 1.0f) },
        { ImGuiCol.Border,                  new Vector4(0.14f, 0.14f, 0.16f, 0.50f) },
        { ImGuiCol.TitleBg,                 new Vector4(0.025f, 0.025f, 0.025f, 0.9f) },
        { ImGuiCol.TitleBgActive,           new Vector4(0.025f, 0.025f, 0.025f, 1.0f) },
        { ImGuiCol.Button,                  new Vector4(1.0f, 1.0f, 1.0f, 0.122f) },
        { ImGuiCol.CheckMark,               new Vector4(0.31f, 0.27f, 0.91f, 1.00f) },
        { ImGuiCol.SliderGrab,              new Vector4(0.43f, 0.40f, 0.56f, 1.00f) },
        { ImGuiCol.SliderGrabActive,        new Vector4(0.55f, 0.50f, 0.80f, 1.00f) },
        { ImGuiCol.ButtonHovered,           new Vector4(0.26f, 0.18f, 0.67f, 1.00f) },
        { ImGuiCol.ButtonActive,            new Vector4(0.21f, 0.14f, 0.55f, 1.00f) },
        { ImGuiCol.SeparatorHovered,        new Vector4(0.21f, 0.15f, 0.55f, 1.00f) },
        { ImGuiCol.SeparatorActive,         new Vector4(0.21f, 0.15f, 0.55f, 1.00f) },
        { ImGuiCol.ResizeGrip,              new Vector4(0.79f, 0.79f, 0.79f, 0.09f) },
        { ImGuiCol.ResizeGripHovered,       new Vector4(0.69f, 0.65f, 0.81f, 0.67f) },
        { ImGuiCol.ResizeGripActive,        new Vector4(0.21f, 0.15f, 0.55f, 1.00f) },
        { ImGuiCol.Tab,                     new Vector4(0.18f, 0.18f, 0.18f, 0.86f) },
        { ImGuiCol.TabHovered,              new Vector4(0.37f, 0.32f, 0.63f, 1.00f) },
        { ImGuiCol.TabActive,               new Vector4(0.26f, 0.18f, 0.67f, 1.00f) },
        { ImGuiCol.PlotLinesHovered,        new Vector4(0.45f, 0.39f, 0.82f, 1.00f) },
        { ImGuiCol.PlotHistogram,           new Vector4(0.26f, 0.18f, 0.67f, 1.00f) },
        { ImGuiCol.PlotHistogramHovered,    new Vector4(0.24f, 0.14f, 0.81f, 1.00f) },
        { ImGuiCol.FrameBg,                 new Vector4(0.12f, 0.12f, 0.12f, 0.54f) },
        { ImGuiCol.FrameBgHovered,          new Vector4(0.20f, 0.20f, 0.20f, 0.40f) },
        { ImGuiCol.FrameBgActive,           new Vector4(0.16f, 0.16f, 0.16f, 0.67f) },
        { ImGuiCol.Header,                  new Vector4(0.10f, 0.10f, 0.10f, 1.00f) },
        { ImGuiCol.HeaderHovered,           new Vector4(0.13f, 0.13f, 0.13f, 0.80f) },
        { ImGuiCol.HeaderActive,            new Vector4(0.13f, 0.13f, 0.13f, 0.80f) },
    };

    public void ApplyStyles()
    {
        if (_stylesApplied) return;


        foreach (var style in StyleVarFloats)
        {
            ImGui.PushStyleVar(style.Key, style.Value);
        }

        foreach (var style in StyleVarVectors)
        {
            ImGui.PushStyleVar(style.Key, style.Value);
        }

        foreach (var style in StyleColors)
        {
            ImGui.PushStyleColor(style.Key, style.Value);
        }

        _stylesApplied = true;
    }

    public void RemoveStyles()
    {
        if (!_stylesApplied) return;

        ImGui.PopStyleVar(StyleVarFloats.Count + StyleVarVectors.Count);
        ImGui.PopStyleColor(StyleColors.Count);

        _stylesApplied = false;
    }

    public void Dispose()
    {
        if (_stylesApplied)
        {
            RemoveStyles();
        }

        if (_messageColorPushed)
        {
            RemoveMessageStyles();
        }
    }
}