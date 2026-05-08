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

    private float _spaceWidth => ImGui.CalcTextSize(" ").X;
    private float _widestTimestampWidth = ImGui.CalcTextSize("12:00 AM").X + ImGui.CalcTextSize(" ").X;

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
            DrawTimestamp(chatEntry.Timestamp);
            SetMessageColor(chatEntry);
            DrawSender(chatEntry);
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

    private void DrawTimestamp(DateTime timestamp)
    {
        var timeString = timestamp.ToString("t");
        var timeStringWidth = ImGui.CalcTextSize(timeString).X;


        ImGui.Text(timeString);
        ImGui.SameLine(0, _widestTimestampWidth - timeStringWidth);
    }

    private void DrawSender(Message message)
    {
        string nameAppend = "";
        string namePrepend = "";

        if (message.ChatType == XivChatType.TellOutgoing)
        {
            ImGui.Text(">> ");
            ImGui.SameLine(0, 0);
        }

        ImGui.Text(namePrepend + message.SenderName + nameAppend);

        if (message.SenderWorld != PlayerCharacter.World)
        {
            ImGui.SameLine(0, 0);
            ImGuiHelpers.DrawIcon(BitmapFontIcon.CrossWorld);
            ImGui.SameLine(0, 0);
            ImGui.Text(message.SenderWorld);
        }

        if (message.ChatType == XivChatType.TellIncoming)
        {
            ImGui.SameLine(0, 0);
            ImGui.Text(" >>");
        }

        ImGui.SameLine(0, _spaceWidth);
    }

    private void DrawMessage(Message message)
    {
        foreach (var messagePart in message.MessageContents)
        {

            float windowRight = ImGui.GetWindowPos().X + ImGui.GetContentRegionAvail().X + ImGui.GetScrollX();
            float rightEdge = ImGui.GetItemRectMax().X; // end of last rendered item

            if (rightEdge + _spaceWidth + messagePart.Width <= windowRight)
            {
                ImGui.SameLine(0, _spaceWidth);
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
