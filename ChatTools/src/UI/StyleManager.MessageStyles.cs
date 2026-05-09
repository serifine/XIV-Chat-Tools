using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChatTools.UI;

public partial class StyleManager
{
    private bool _messageColorPushed = false;

    public void ApplyMessageStyles(XivChatType chatType)
    {
        if (Configuration.DisableCustomChatColors) return;

        var category = GetColorCategoryFromChatType(chatType);

        if (category.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ColorConfigurations.GetColor(category.Value));
            _messageColorPushed = true;
        }
    }

    public void RemoveMessageStyles()
    {
        if (_messageColorPushed)
        {
            ImGui.PopStyleColor();
            _messageColorPushed = false;
        }
    }

    private ColorCategory? GetColorCategoryFromChatType(XivChatType chatType) => chatType switch
    {
        XivChatType.Say => ColorCategory.Say,
        XivChatType.Yell => ColorCategory.Yell,
        XivChatType.Shout => ColorCategory.Shout,
        XivChatType.Party => ColorCategory.Party,
        XivChatType.Alliance => ColorCategory.Alliance,
        XivChatType.FreeCompany => ColorCategory.FreeCompany,
        XivChatType.CustomEmote or
        XivChatType.StandardEmote => ColorCategory.Emote,
        XivChatType.TellIncoming or
        XivChatType.TellOutgoing => ColorCategory.Tell,
        XivChatType.Ls1 => ColorCategory.LS1,
        XivChatType.Ls2 => ColorCategory.LS2,
        XivChatType.Ls3 => ColorCategory.LS3,
        XivChatType.Ls4 => ColorCategory.LS4,
        XivChatType.Ls5 => ColorCategory.LS5,
        XivChatType.Ls6 => ColorCategory.LS6,
        XivChatType.Ls7 => ColorCategory.LS7,
        XivChatType.Ls8 => ColorCategory.LS8,
        XivChatType.CrossLinkShell1 => ColorCategory.CWLS1,
        XivChatType.CrossLinkShell2 => ColorCategory.CWLS2,
        XivChatType.CrossLinkShell3 => ColorCategory.CWLS3,
        XivChatType.CrossLinkShell4 => ColorCategory.CWLS4,
        XivChatType.CrossLinkShell5 => ColorCategory.CWLS5,
        XivChatType.CrossLinkShell6 => ColorCategory.CWLS6,
        XivChatType.CrossLinkShell7 => ColorCategory.CWLS7,
        XivChatType.CrossLinkShell8 => ColorCategory.CWLS8,
        _ => null
    };
}