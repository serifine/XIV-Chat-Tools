using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using ChatTools.Helpers;
using ChatTools.IO;
using ChatTools.Services;
using ChatTools.UI.Components;
using ChatTools.Models;
using Dalamud.Game.ClientState.Conditions;

namespace ChatTools.UI.Windows;

// TODO: This class is getting pretty large. Split into multiple windows or components.

public class MainWindow : Window
{
    private readonly Plugin _plugin;
    private readonly FocusTargetTabComponent _focusTargetTabComponent;
    private readonly WatcherConfigurationComponent _watcherConfigurationComponent;

    private TabControllerService TabController => _plugin.TabController;
    private WindowManagerService WindowManager => _plugin.WindowManagerService;
    private Configuration Configuration => _plugin.Configuration;
    private PluginStateService PluginState => _plugin.PluginState;
    private MessageService MessageService => _plugin.MessageService;
    private IPluginLog Logger => Plugin.Logger;

    private FocusTabComponent _focusTabComponent;

    private string _activeTabKey = "focus_target";
    private float _tabScrollOffset = 0f;

    private const float TabArrowButtonWidth = 22f;
    private const float TabScrollStep = 80f;

    internal MainWindow(Plugin plugin) : base($"Chat Tools###ChatToolsMainWindow")
    {
        _plugin = plugin;
        _focusTargetTabComponent = new(_plugin);
        _watcherConfigurationComponent = new(_plugin);

        Size = new Vector2(450, 50);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDocking;

        _focusTabComponent = new(plugin);

        if (Plugin.PluginInterface.IsDev)
        {
            var tabId = TabController.AddFocusTab(new PlayerIdentifier("Tessa Elran", "Mateus"));
            _activeTabKey = tabId.ToString();
        }
    }

    public override bool DrawConditions()
    {
        return !Plugin.Condition[ConditionFlag.OccupiedInCutSceneEvent] &&
               !Plugin.Condition[ConditionFlag.PlayingMiniGame] &&
               !Plugin.Condition[ConditionFlag.OccupiedInEvent] &&
               !Plugin.Condition[ConditionFlag.OccupiedInQuestEvent] &&
               !Plugin.Condition[ConditionFlag.BetweenAreas] &&
               !Plugin.Condition[ConditionFlag.Crafting];
    }

    public override void PreDraw()
    {
        _focusTargetTabComponent.PreDraw();

        base.PreDraw();
    }

    public override void Draw()
    {
        try
        {
            DrawInterface();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error drawing Chat Tools window.");
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();
    }

    private void DrawInterface()
    {
        using (new FullWidthContainer("ToolbarContainer", outerMargin: 0))
        {
            var style = ImGui.GetStyle();

            float buttonPanelWidth = (24 * 4) + (4 * 3); // Formula is (numButtons * buttonWidth) + (numSpacings * itemSpacing)
            float totalWidth = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y;

            DrawTabs(totalWidth - buttonPanelWidth - style.ItemSpacing.X, height);
            ImGui.SameLine();
            DrawActionButtons(buttonPanelWidth, 24, height);
        }

        DrawTabContent();
    }

    private void DrawPopups()
    {
        ImGui.SetNextWindowSize(new Vector2(320, 0));
        using var popup = ImRaii.Popup("Alerts", ImGuiWindowFlags.NoResize);

        if (popup)
        {
            _watcherConfigurationComponent.Draw();
        }
    }

    private void DrawTabs(float panelWidth, float buttonHeight)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 0, 0));
        var frame = ImGui.BeginChildFrame(ImGui.GetID("TabFrame"), new Vector2(panelWidth, buttonHeight), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(2);

        if (frame)
        {
            DrawTabPanel();
            ImGui.EndChildFrame();
        }
    }

    private void DrawActionButtons(float panelWidth, float buttonWidth, float buttonHeight)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 0, 0));
        var frame = ImGui.BeginChildFrame(ImGui.GetID("ActionButtonFrame"), new Vector2(panelWidth, buttonHeight), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(2);

        if (frame)
        {
            DrawPopups();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 4));
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f));

            if (ImGuiComponents.IconButton(FontAwesomeIcon.PlusCircle, new Vector2(buttonWidth, buttonHeight)))
                TabController.AddFocusTab();

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("New Watch Tab");

            ImGui.SameLine();

            if (ImGuiComponents.IconButton(FontAwesomeIcon.Search, new Vector2(buttonWidth, buttonHeight)))
                WindowManager.SearchWindow.Toggle();

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Search Messages");

            ImGui.SameLine();

            if (ImGuiComponents.IconButton(FontAwesomeIcon.Bell, new Vector2(buttonWidth, buttonHeight)))
                ImGui.OpenPopup("Alerts");

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Chat Alerts");

            ImGui.SameLine();

            if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog, new Vector2(buttonWidth, buttonHeight)))
                WindowManager.SettingsWindow.Toggle();

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Settings");

            ImGui.PopStyleVar(2);
            ImGui.EndChildFrame();
        }
    }



    private float MeasureTabButtonsWidth(List<Models.Tabs.FocusTab> tabs)
    {
        var style = ImGui.GetStyle();
        float framePadX2 = style.FramePadding.X * 2;
        const float closeGap = 4f;

        float total = ImGui.CalcTextSize("Current Target").X + framePadX2 + 6 * 2 + 8;

        foreach (var tab in tabs)
        {
            total += style.ItemSpacing.X;
            total += ImGui.CalcTextSize(tab.Title).X + ImGui.CalcTextSize("X").X + closeGap + framePadX2;
        }

        return total;
    }

    private void DrawTabPanel()
    {
        var style = ImGui.GetStyle();
        var tabs = TabController.GetFocusTabs();
        float totalWidth = ImGui.GetWindowWidth();
        float windowHeight = ImGui.GetWindowHeight();

        // Measure total width all tab buttons would occupy
        float totalTabsWidth = MeasureTabButtonsWidth(tabs);

        bool needsScroll = totalTabsWidth > totalWidth;

        float scrollRegionWidth = needsScroll
            ? totalWidth - TabArrowButtonWidth * 2
            : totalWidth;

        float maxScroll = needsScroll ? Math.Max(0f, totalTabsWidth - scrollRegionWidth) : 0f;
        _tabScrollOffset = Math.Clamp(_tabScrollOffset, 0f, maxScroll);

        bool showLeftArrow = needsScroll && _tabScrollOffset > 0.5f;
        bool showRightArrow = needsScroll && _tabScrollOffset < maxScroll - 0.5f;

        // Left arrow or invisible spacer
        if (needsScroll)
        {
            if (showLeftArrow)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1, 1, 1, 0.05f));
                if (ImGui.Button("<##TabScrollLeft", new Vector2(TabArrowButtonWidth, windowHeight)))
                    _tabScrollOffset = Math.Max(0f, _tabScrollOffset - TabScrollStep);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.Dummy(new Vector2(TabArrowButtonWidth, windowHeight));
            }
            ImGui.SameLine(0, 0);
        }

        // Scrollable tab button region
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.SetNextWindowContentSize(new Vector2(totalTabsWidth, 0));
        ImGui.BeginChild("##TabScrollRegion",
            new Vector2(scrollRegionWidth, windowHeight),
            false,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        ImGui.SetScrollX(_tabScrollOffset);

        DrawTabButton("focus_target", "Current Target");

        foreach (var tab in tabs)
        {
            ImGui.SameLine(0, style.ItemSpacing.X);
            DrawTabButton(tab.TabId.ToString(), tab.Title, () =>
            {
                if (_activeTabKey == tab.TabId.ToString())
                    _activeTabKey = "focus_target";
                tab.Close();
            });

            if (ImGui.BeginPopupContextItem($"##TabCtx_{tab.TabId}"))
            {
                ImGui.SetNextItemWidth(160);
                ImGui.InputText($"###{tab.TabId}Rename", ref tab.Title, 64);
                if (ImGui.MenuItem("Close Tab"))
                {
                    if (_activeTabKey == tab.TabId.ToString())
                        _activeTabKey = "focus_target";
                    tab.Close();
                }
                ImGui.EndPopup();
            }
        }

        ImGui.EndChild();
        ImGui.PopStyleVar();

        // Right arrow or invisible spacer
        if (needsScroll)
        {
            ImGui.SameLine(0, 0);
            if (showRightArrow)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1, 1, 1, 0.05f));
                if (ImGui.Button(">##TabScrollRight", new Vector2(TabArrowButtonWidth, windowHeight)))
                    _tabScrollOffset = Math.Min(maxScroll, _tabScrollOffset + TabScrollStep);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.Dummy(new Vector2(TabArrowButtonWidth, windowHeight));
            }
        }
    }

    private void DrawTabButton(string key, string label, Action? onClose = null)
    {
        bool isActive = _activeTabKey == key;
        var style = ImGui.GetStyle();

        float windowHeight = ImGui.GetWindowHeight() - style.WindowPadding.Y;
        float padX = 6;
        float windowPadY = style.WindowPadding.Y;

        float labelWidth = ImGui.CalcTextSize(label).X;

        const float closeGap = 4f;
        float closeTextWidth = onClose != null ? ImGui.CalcTextSize("x").X + closeGap : 0f;
        float totalWidth = labelWidth + closeTextWidth + (padX * 2);

        var buttonTopLeft = ImGui.GetCursorScreenPos();

        if (isActive)
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive));

        // ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f));
        bool buttonClicked = ImGui.Button($"##{key}_tab", new Vector2(totalWidth, windowHeight));

        if (isActive)
            ImGui.PopStyleColor();

        var drawList = ImGui.GetWindowDrawList();
        drawList.AddText(
            new Vector2(buttonTopLeft.X + padX, buttonTopLeft.Y + windowPadY + 3),
            ImGui.GetColorU32(ImGuiCol.Text),
            label
        );

        if (onClose != null)
        {
            float closeX = buttonTopLeft.X + padX + labelWidth + closeGap;
            float closeY = buttonTopLeft.Y + windowPadY + 3;

            var mousePos = ImGui.GetMousePos();
            bool mouseOverClose =
                mousePos.X >= closeX - 2f &&
                mousePos.X <= closeX + ImGui.CalcTextSize("X").X + 2f &&
                mousePos.Y >= buttonTopLeft.Y &&
                mousePos.Y <= buttonTopLeft.Y + windowHeight;

            uint closeColor = mouseOverClose
                ? ImGui.GetColorU32(ImGuiCol.Text)
                : ImGui.GetColorU32(ImGuiCol.TextDisabled);

            drawList.AddText(new Vector2(closeX, closeY), closeColor, "X");

            if (buttonClicked)
            {
                if (mouseOverClose)
                    onClose();
                else
                    _activeTabKey = key;
            }
        }
        else if (buttonClicked)
        {
            _activeTabKey = key;
        }
    }

    private void DrawTabContent()
    {
        if (_activeTabKey == "focus_target")
        {
            _focusTargetTabComponent.Draw();
            return;
        }

        var activeTab = TabController.GetFocusTabs().Find(t => t.TabId.ToString() == _activeTabKey);

        if (activeTab == null)
        {
            _activeTabKey = "focus_target";
            _focusTargetTabComponent.Draw();
        }
        else
        {
            _focusTabComponent.Draw(activeTab);
        }
    }

    private void DrawSelectedTargetTab()
    {
    }
}
