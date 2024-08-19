

using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility;
using ImGuiNET;
using XIVChatTools;
using XIVChatTools.Models;
using XIVChatTools.Services;

namespace XIVChatTools.UI;

public class MessagePanel {
    private readonly Plugin _plugin;

    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;

    public MessagePanel(Plugin plugin)
    {
        _plugin = plugin;
    }

    public void Draw(List<ChatEntry> messages)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 8));

        var contentRegionAvail = ImGui.GetContentRegionAvail();

        ImGui.BeginChild("Messages", new Vector2(contentRegionAvail.X, contentRegionAvail.Y), true, ImGuiWindowFlags.None);

        var isChatAtBottom = ImGui.GetScrollY() == ImGui.GetScrollMaxY();

        if (ImGui.BeginTable("table1", 2, ImGuiTableFlags.NoHostExtendX))
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            foreach (var chatEntry in messages)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                SetNameColor(chatEntry);

                if (Configuration.SplitDateAndNames == true)
                {
                    ImGui.Text(chatEntry.SenderName);
                    ImGui.Text(chatEntry.DateSent.ToShortTimeString());
                }
                else
                {
                    ImGui.Text(chatEntry.DateSent.ToShortTimeString() + " " + chatEntry.SenderName + ": ");
                }
                ImGui.PopStyleColor();

                ImGui.TableSetColumnIndex(1);

                SetMessageColor(chatEntry);
                ImGuiHelpers.SafeTextWrapped(chatEntry.Message);
                ImGui.PopStyleColor();
            }

            ImGui.EndTable();
        }

        if (isChatAtBottom == true)
        {
            ImGui.SetScrollHereY(1.0f);
        }

        ImGui.EndChild();

        ImGui.PopStyleVar();
    }
    
    private void SetNameColor(ChatEntry chatEntry)
    {
        if (chatEntry.SenderName == PluginState.GetPlayerName())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.CharacterNameColor);
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.NormalChatColor);
        }
    }

    private void SetMessageColor(ChatEntry chatEntry)
    {
        if (Configuration.DisableCustomChatColors)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.NormalChatColor);
        }
        else if (chatEntry.ChatType == XivChatType.CustomEmote || chatEntry.ChatType == XivChatType.StandardEmote)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.EmoteColor);
        }
        else if (chatEntry.ChatType == XivChatType.TellIncoming || chatEntry.ChatType == XivChatType.TellOutgoing)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.TellColor);
        }
        else if (chatEntry.ChatType == XivChatType.Party)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.PartyColor);
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.NormalChatColor);
        }
    }
}
