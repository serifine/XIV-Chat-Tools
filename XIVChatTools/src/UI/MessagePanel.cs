

using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility;
using ImGuiNET;
using XIVChatTools;
using XIVChatTools.Database.Models;
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

    public void Draw(List<Message> messages)
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
                    ImGui.Text(chatEntry.Timestamp.ToShortTimeString());
                }
                else
                {
                    ImGui.Text(chatEntry.Timestamp.ToShortTimeString() + " " + chatEntry.SenderName + ": ");
                }
                ImGui.PopStyleColor();

                ImGui.TableSetColumnIndex(1);

                SetMessageColor(chatEntry);
                ImGuiHelpers.SafeTextWrapped(chatEntry.MessageContents);
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
    
    private void SetNameColor(Message message)
    {
        if (message.SenderName == Helpers.PlayerCharacter.Name)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.CharacterNameColor);
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.NormalChatColor);
        }
    }

    private void SetMessageColor(Message message)
    {
        if (Configuration.DisableCustomChatColors)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.NormalChatColor);
        }
        else if (message.ChatType == XivChatType.CustomEmote || message.ChatType == XivChatType.StandardEmote)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.EmoteColor);
        }
        else if (message.ChatType == XivChatType.TellIncoming || message.ChatType == XivChatType.TellOutgoing)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.TellColor);
        }
        else if (message.ChatType == XivChatType.Party)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.PartyColor);
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Configuration.NormalChatColor);
        }
    }
}
