

using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ChatTools.DB.Models;
using ChatTools.Helpers;
using ChatTools.Services;

namespace ChatTools.UI;

public class MessagePanel
{
    private readonly Plugin _plugin;

    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;

    public MessagePanel(Plugin plugin)
    {
        _plugin = plugin;
    }

    public void Draw(List<Message> messages)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 8));

        var contentRegionAvail = ImGui.GetContentRegionAvail();

        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 0, 0.1f));
        ImGui.BeginChild("Messages", new Vector2(contentRegionAvail.X, contentRegionAvail.Y), true, ImGuiWindowFlags.None);
        ImGui.PopStyleColor();

        var isChatAtBottom = ImGui.GetScrollY() == ImGui.GetScrollMaxY();

        foreach (var chatEntry in messages)
        {
            // TODO: Re-implement name coloring and prepending/appending of arrows for tells. Will likely need to be moved.
            // string nameAppend = "";
            // string namePrepend = "";

            // ImGui.TableNextRow();
            // ImGui.TableSetColumnIndex(0);

            // SetNameColor(chatEntry);

            // if (chatEntry.ChatType == XivChatType.TellOutgoing)
            // {
            //     namePrepend = ">>";
            // }

            // if (chatEntry.ChatType == XivChatType.TellIncoming)
            // {
            //     nameAppend = ">>";
            // }

            // ImGui.Text($"{chatEntry.Timestamp.ToShortTimeString()} {namePrepend}{chatEntry.SenderName}{nameAppend}: ");

            // ImGui.PopStyleColor();

            // ImGui.TableSetColumnIndex(1);

            SetMessageColor(chatEntry);
            DrawMessage(chatEntry);
            ImGui.PopStyleColor();
        }

        if (isChatAtBottom == true)
        {
            ImGui.SetScrollHereY(1.0f);
        }

        ImGui.EndChild();

        ImGui.PopStyleVar();
    }

    private void DrawMessage(Message message)
    {
        float spaceWidth = ImGui.CalcTextSize(" ").X;

        // Date Rendering =====================================================
        ImGui.Text(message.Timestamp.ToString("t"));
        ImGui.SameLine(0, spaceWidth);

        // message.SenderName
        // Player Name and World ==============================================
        // if (sender is PlayerPayload playerPayload)
        // {
        ImGui.Text(message.SenderName);

        if (true)
        {
            ImGui.SameLine(0, 0);
            DrawHelpers.DrawIcon(BitmapFontIcon.CrossWorld);
            ImGui.SameLine(0, 0);
            ImGui.Text(message.SenderWorld);
        }
        // }
        // else if (sender is TextPayload textPayload)
        // {
        //     ImGui.Text(textPayload.Text);
        // }

        // Message Contents ===================================================
        foreach (var messagePart in message.MessageContents)
        {

            float windowRight = ImGui.GetWindowPos().X + ImGui.GetContentRegionAvail().X + ImGui.GetScrollX();
            float rightEdge = ImGui.GetItemRectMax().X; // end of last rendered item

            if (rightEdge + spaceWidth + messagePart.Width <= windowRight)
            {
                ImGui.SameLine(0, spaceWidth);
            }

            messagePart.Draw();
        }
    }

    private void SetMessageColor(Message message)
    {
        if (Configuration.DisableCustomChatColors)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ColorConfigurations.GetColor(ColorCategory.Say));
        }
        else if (message.ChatType == XivChatType.CustomEmote || message.ChatType == XivChatType.StandardEmote)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ColorConfigurations.GetColor(ColorCategory.Emote));
        }
        else if (message.ChatType == XivChatType.TellIncoming || message.ChatType == XivChatType.TellOutgoing)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ColorConfigurations.GetColor(ColorCategory.Tell));
        }
        else if (message.ChatType == XivChatType.Party)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ColorConfigurations.GetColor(ColorCategory.Party));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ColorConfigurations.GetColor(ColorCategory.Say));
        }
    }
}
