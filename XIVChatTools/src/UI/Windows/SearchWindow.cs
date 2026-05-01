using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using XIVChatTools.DB.Models;
using XIVChatTools.Services;

namespace XIVChatTools.UI.Windows;

public class SearchWindow : Window
{
    private readonly Plugin _plugin;
    private readonly MessagePanel _messagePanel;

    private PluginStateService PluginState => _plugin.PluginState;
    private MessageService MessageService => _plugin.MessageService;

    private List<Message> _searchMessages = [];
    private string _searchText = "";

    internal SearchWindow(Plugin plugin) : base($"Search###ChatToolsSearchWindow")
    {
        _plugin = plugin;
        _messagePanel = new(_plugin);

        _searchMessages = MessageService.GetAllMessages();

        Size = new Vector2(450, 600);
        SizeConstraints = new WindowSizeConstraints() { MinimumSize = new Vector2(450, 600), MaximumSize = new Vector2(700, 1200) };
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDocking;
    }

    private void DrawInterface()
    {
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputTextWithHint("", "Search Messages", ref _searchText, 24096))
        {
            _searchMessages = MessageService.SearchMessages(_searchText);
        }

        ImGui.Separator();

        if (_searchMessages.Count > 0)
        {
            _messagePanel.Draw(_searchMessages);
        }
        else
        {
            ImGui.Text("No messages found.");
        }
    }

    public override void Draw()
    {
        DrawInterface();
    }
}
