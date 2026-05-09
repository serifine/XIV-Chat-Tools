using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Config;
using Dalamud.Interface.Windowing;
using ChatTools.UI.Components;

namespace ChatTools.UI.Windows;

public partial class SettingsWindow
{
    private Vector4 SayColor = ColorConfigurations.GetColor(ColorCategory.Say);
    private Vector4 EmoteColor = ColorConfigurations.GetColor(ColorCategory.Emote);
    private Vector4 PartyColor = ColorConfigurations.GetColor(ColorCategory.Party);
    private Vector4 TellColor = ColorConfigurations.GetColor(ColorCategory.Tell);
    private Vector4 YellColor = ColorConfigurations.GetColor(ColorCategory.Yell);
    private Vector4 ShoutColor = ColorConfigurations.GetColor(ColorCategory.Shout);
    private Vector4 WatchColor = ColorConfigurations.GetColor(ColorCategory.Watch);
    private Vector4 AllianceColor = ColorConfigurations.GetColor(ColorCategory.Alliance);
    private Vector4 FreeCompanyColor = ColorConfigurations.GetColor(ColorCategory.FreeCompany);
    private Vector4 LS1Color = ColorConfigurations.GetColor(ColorCategory.LS1);
    private Vector4 LS2Color = ColorConfigurations.GetColor(ColorCategory.LS2);
    private Vector4 LS3Color = ColorConfigurations.GetColor(ColorCategory.LS3);
    private Vector4 LS4Color = ColorConfigurations.GetColor(ColorCategory.LS4);
    private Vector4 LS5Color = ColorConfigurations.GetColor(ColorCategory.LS5);
    private Vector4 LS6Color = ColorConfigurations.GetColor(ColorCategory.LS6);
    private Vector4 LS7Color = ColorConfigurations.GetColor(ColorCategory.LS7);
    private Vector4 LS8Color = ColorConfigurations.GetColor(ColorCategory.LS8);
    private Vector4 CWLS1Color = ColorConfigurations.GetColor(ColorCategory.CWLS1);
    private Vector4 CWLS2Color = ColorConfigurations.GetColor(ColorCategory.CWLS2);
    private Vector4 CWLS3Color = ColorConfigurations.GetColor(ColorCategory.CWLS3);
    private Vector4 CWLS4Color = ColorConfigurations.GetColor(ColorCategory.CWLS4);
    private Vector4 CWLS5Color = ColorConfigurations.GetColor(ColorCategory.CWLS5);
    private Vector4 CWLS6Color = ColorConfigurations.GetColor(ColorCategory.CWLS6);
    private Vector4 CWLS7Color = ColorConfigurations.GetColor(ColorCategory.CWLS7);
    private Vector4 CWLS8Color = ColorConfigurations.GetColor(ColorCategory.CWLS8);


    private void DrawColorSettingsPanel()
    {
        if (ImGui.Checkbox("Disable Custom Chat Colors", ref Configuration.DisableCustomChatColors))
        {
            Configuration.Save();
        }

        ImGui.SameLine(0, 24);

        if (ImGui.Button("Sync Colors With Game"))
        {
            Configuration.CustomChatColors[ColorCategory.Say] =
                GetGameChatColor(UiConfigOption.ColorSay) ??
                ColorConfigurations.DefaultColors[ColorCategory.Say];

            Configuration.CustomChatColors[ColorCategory.Emote] =
                GetGameChatColor(UiConfigOption.ColorEmote) ??
                ColorConfigurations.DefaultColors[ColorCategory.Emote];

            Configuration.CustomChatColors[ColorCategory.Tell] =
                GetGameChatColor(UiConfigOption.ColorTell) ??
                ColorConfigurations.DefaultColors[ColorCategory.Tell];

            Configuration.CustomChatColors[ColorCategory.Party] =
                GetGameChatColor(UiConfigOption.ColorParty) ??
                ColorConfigurations.DefaultColors[ColorCategory.Party];

            Configuration.CustomChatColors[ColorCategory.Shout] =
                GetGameChatColor(UiConfigOption.ColorShout) ??
                ColorConfigurations.DefaultColors[ColorCategory.Shout];

            Configuration.CustomChatColors[ColorCategory.Yell] =
                GetGameChatColor(UiConfigOption.ColorYell) ??
                ColorConfigurations.DefaultColors[ColorCategory.Yell];

            Configuration.CustomChatColors[ColorCategory.Alliance] =
                GetGameChatColor(UiConfigOption.ColorAlliance) ??
                ColorConfigurations.DefaultColors[ColorCategory.Alliance];

            Configuration.CustomChatColors[ColorCategory.FreeCompany] =
                GetGameChatColor(UiConfigOption.ColorFCompany) ??
                ColorConfigurations.DefaultColors[ColorCategory.FreeCompany];

            Configuration.CustomChatColors[ColorCategory.LS1] =
                GetGameChatColor(UiConfigOption.ColorLS1) ??
                ColorConfigurations.DefaultColors[ColorCategory.LS1];

            Configuration.CustomChatColors[ColorCategory.LS2] =
                GetGameChatColor(UiConfigOption.ColorLS2) ??
                ColorConfigurations.DefaultColors[ColorCategory.LS2];
            
            Configuration.CustomChatColors[ColorCategory.LS3] =
                GetGameChatColor(UiConfigOption.ColorLS3) ??
                ColorConfigurations.DefaultColors[ColorCategory.LS3];
            
            Configuration.CustomChatColors[ColorCategory.LS4] =
                GetGameChatColor(UiConfigOption.ColorLS4) ??
                ColorConfigurations.DefaultColors[ColorCategory.LS4];
            
            Configuration.CustomChatColors[ColorCategory.LS5] =
                GetGameChatColor(UiConfigOption.ColorLS5) ??
                ColorConfigurations.DefaultColors[ColorCategory.LS5];
            
            Configuration.CustomChatColors[ColorCategory.LS6] =
                GetGameChatColor(UiConfigOption.ColorLS6) ??
                ColorConfigurations.DefaultColors[ColorCategory.LS6];
            
            Configuration.CustomChatColors[ColorCategory.LS7] =
                GetGameChatColor(UiConfigOption.ColorLS7) ??
                ColorConfigurations.DefaultColors[ColorCategory.LS7];
            
            Configuration.CustomChatColors[ColorCategory.LS8] =
                GetGameChatColor(UiConfigOption.ColorLS8) ??
                ColorConfigurations.DefaultColors[ColorCategory.LS8];

            Configuration.CustomChatColors[ColorCategory.CWLS1] =
                GetGameChatColor(UiConfigOption.ColorCWLS) ??
                ColorConfigurations.DefaultColors[ColorCategory.CWLS1];

            Configuration.CustomChatColors[ColorCategory.CWLS2] =
                GetGameChatColor(UiConfigOption.ColorCWLS2) ??
                ColorConfigurations.DefaultColors[ColorCategory.CWLS2];

            Configuration.CustomChatColors[ColorCategory.CWLS3] =
                GetGameChatColor(UiConfigOption.ColorCWLS3) ??
                ColorConfigurations.DefaultColors[ColorCategory.CWLS3];

            Configuration.CustomChatColors[ColorCategory.CWLS4] =
                GetGameChatColor(UiConfigOption.ColorCWLS4) ??
                ColorConfigurations.DefaultColors[ColorCategory.CWLS4];

            Configuration.CustomChatColors[ColorCategory.CWLS5] =
                GetGameChatColor(UiConfigOption.ColorCWLS5) ??
                ColorConfigurations.DefaultColors[ColorCategory.CWLS5];
            
            Configuration.CustomChatColors[ColorCategory.CWLS6] =
                GetGameChatColor(UiConfigOption.ColorCWLS6) ??
                ColorConfigurations.DefaultColors[ColorCategory.CWLS6];

            Configuration.CustomChatColors[ColorCategory.CWLS7] =
                GetGameChatColor(UiConfigOption.ColorCWLS7) ??
                ColorConfigurations.DefaultColors[ColorCategory.CWLS7];

            Configuration.CustomChatColors[ColorCategory.CWLS8] =
                GetGameChatColor(UiConfigOption.ColorCWLS8) ??
                ColorConfigurations.DefaultColors[ColorCategory.CWLS8];


            Configuration.Save();
            ReloadColors();
        }

        ImGui.Spacing();

        if (ImGui.BeginTable("##chatColorTable", 2, ImGuiTableFlags.None))
        {
            ImGui.TableSetupColumn("##chatColorColumn1", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##chatColorColumn2", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            if (ImGui.ColorEdit4("Normal Message Color", ref SayColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.Say] = SayColor;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Emote Color", ref EmoteColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.Emote] = EmoteColor;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Tell Color", ref TellColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.Tell] = TellColor;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Yell Color", ref YellColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.Yell] = YellColor;
                Configuration.Save();
            }

            ImGui.TableNextColumn();

            if (ImGui.ColorEdit4("Party Chat Color", ref PartyColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.Party] = PartyColor;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Alliance Color", ref AllianceColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.Alliance] = AllianceColor;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Free Company Color", ref FreeCompanyColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.FreeCompany] = FreeCompanyColor;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Shout Color", ref ShoutColor, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.Shout] = ShoutColor;
                Configuration.Save();
            }

            ImGui.EndTable();
        }

        ImGui.Spacing();

        if (ImGui.BeginTable("##linkshellColorTable", 2, ImGuiTableFlags.None))
        {
            ImGui.TableSetupColumn("##lsColumn", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##cwlsColumn", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            #region Linkshell Colors
            if (ImGui.ColorEdit4("Linkshell 1", ref LS1Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.LS1] = LS1Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Linkshell 2", ref LS2Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.LS2] = LS2Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Linkshell 3", ref LS3Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.LS3] = LS3Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Linkshell 4", ref LS4Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.LS4] = LS4Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Linkshell 5", ref LS5Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.LS5] = LS5Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Linkshell 6", ref LS6Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.LS6] = LS6Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Linkshell 7", ref LS7Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.LS7] = LS7Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Linkshell 8", ref LS8Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.LS8] = LS8Color;
                Configuration.Save();
            }
            #endregion

            ImGui.TableNextColumn();

            #region Linkshell Colors
            if (ImGui.ColorEdit4("Cross-world Linkshell 1", ref CWLS1Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.CWLS1] = CWLS1Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Cross-world Linkshell 2", ref CWLS2Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.CWLS2] = CWLS2Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Cross-world Linkshell 3", ref CWLS3Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.CWLS3] = CWLS3Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Cross-world Linkshell 4", ref CWLS4Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.CWLS4] = CWLS4Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Cross-world Linkshell 5", ref CWLS5Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.CWLS5] = CWLS5Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Cross-world Linkshell 6", ref CWLS6Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.CWLS6] = CWLS6Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Cross-world Linkshell 7", ref CWLS7Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.CWLS7] = CWLS7Color;
                Configuration.Save();
            }

            if (ImGui.ColorEdit4("Cross-world Linkshell 8", ref CWLS8Color, ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoInputs))
            {
                Configuration.CustomChatColors[ColorCategory.CWLS8] = CWLS8Color;
                Configuration.Save();
            }
            #endregion

            ImGui.EndTable();
        }
    }

    private void ReloadColors()
    {
        SayColor = ColorConfigurations.GetColor(ColorCategory.Say);
        EmoteColor = ColorConfigurations.GetColor(ColorCategory.Emote);
        PartyColor = ColorConfigurations.GetColor(ColorCategory.Party);
        TellColor = ColorConfigurations.GetColor(ColorCategory.Tell);
        WatchColor = ColorConfigurations.GetColor(ColorCategory.Watch);
        AllianceColor = ColorConfigurations.GetColor(ColorCategory.Alliance);
        FreeCompanyColor = ColorConfigurations.GetColor(ColorCategory.FreeCompany);
        LS1Color = ColorConfigurations.GetColor(ColorCategory.LS1);
        LS2Color = ColorConfigurations.GetColor(ColorCategory.LS2);
        LS3Color = ColorConfigurations.GetColor(ColorCategory.LS3);
        LS4Color = ColorConfigurations.GetColor(ColorCategory.LS4);
        LS5Color = ColorConfigurations.GetColor(ColorCategory.LS5);
        LS6Color = ColorConfigurations.GetColor(ColorCategory.LS6);
        LS7Color = ColorConfigurations.GetColor(ColorCategory.LS7);
        LS8Color = ColorConfigurations.GetColor(ColorCategory.LS8);
        CWLS1Color = ColorConfigurations.GetColor(ColorCategory.CWLS1);
        CWLS2Color = ColorConfigurations.GetColor(ColorCategory.CWLS2);
        CWLS3Color = ColorConfigurations.GetColor(ColorCategory.CWLS3);
        CWLS4Color = ColorConfigurations.GetColor(ColorCategory.CWLS4);
        CWLS5Color = ColorConfigurations.GetColor(ColorCategory.CWLS5);
        CWLS6Color = ColorConfigurations.GetColor(ColorCategory.CWLS6);
        CWLS7Color = ColorConfigurations.GetColor(ColorCategory.CWLS7);
        CWLS8Color = ColorConfigurations.GetColor(ColorCategory.CWLS8);
    }

    private Vector4? GetGameChatColor(UiConfigOption option)
    {
        Plugin.GameConfig.TryGet(option, out uint color);

        if (color == 0)
            return null;

        var rgb = color & 0xFFFFFF;

        float b = (rgb & 0xFF) / 255f;
        float g = ((rgb >> 8) & 0xFF) / 255f;
        float r = ((rgb >> 16) & 0xFF) / 255f;

        return new Vector4(r, g, b, 1f);
    }
}