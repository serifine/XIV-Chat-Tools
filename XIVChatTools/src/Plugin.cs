using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using Dalamud.IoC;
using Dalamud;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using XIVChatTools.Services;
using Lumina.Excel.Sheets;
using Lumina.Excel;
using XIVChatTools.Database;
using Microsoft.EntityFrameworkCore;
using XIVChatTools.Database.Models;
using System.Threading.Tasks;
using XIVChatTools.Helpers;

namespace XIVChatTools;

public class Plugin : IDalamudPlugin
{
    public static string Name => "Chat Tools";

    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;

    internal readonly ExcelSheet<World> WorldSheet;
    internal readonly PluginStateService PluginState;
    internal readonly MessageService MessageService;
    internal readonly WindowManagerService WindowManagerService;
    internal readonly TabControllerService TabController;
    internal readonly Configuration Configuration;
    internal readonly ChatToolsDbContext DbContext;

    private readonly List<string> commandAliases = [
        "/chattools",
        "/ctools",
        "/ct"
    ];

    private readonly List<string> settingsArgumentAliases = [
        "settings",
        "config"
    ];

    public Plugin()
    {

#if DEBUG
        Logger.Debug("Chat Tools initialized in debug mode.");
#endif

        try
        {
            PlayerCharacter.UpdatePlayerCharacter();

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            // to be removed later
            Configuration.Initialize(PluginInterface);

            DbContext = InitializeDbContext();

            PluginState = new PluginStateService(this);
            MessageService = new MessageService(this);
            TabController = new TabControllerService(this);
            WindowManagerService = new WindowManagerService(this);

            WorldSheet = DataManager.GetExcelSheet<World>()!;

            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;

            PluginInterface.UiBuilder.Draw += OnDrawUI;
            PluginInterface.UiBuilder.OpenMainUi += OnOpenMainUI;
            PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUI;

            ChatGui.ChatMessageUnhandled += MessageService.HandleChatMessage;

            foreach (string commandAlias in commandAliases)
            {
                CommandManager.AddHandler(commandAlias, new CommandInfo(OnCommand)
                {
                    HelpMessage = commandAliases.First() == commandAlias ?
                      "Opens the Chat Tools window." : "Alias for /chattools."
                });
            }

#if DEBUG
            Logger.Debug("Opening main window for debug.");
            WindowManagerService.MainWindow.IsOpen = true;
#endif
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    #region Event Handlers

    private void OnCommand(string command, string args)
    {
        if (settingsArgumentAliases.Contains(args.ToLower()))
        {
            WindowManagerService.SettingsWindow.IsOpen = !WindowManagerService.SettingsWindow.IsOpen;
        }
        else
        {
            OnOpenMainUI();
        }
    }

    private void OnDrawUI()
    {
        WindowManagerService.Draw();

        PostDrawEvents();
    }

    private void OnOpenMainUI()
    {
        WindowManagerService.ToolbarWindow.Toggle();
    }

    private void OnOpenConfigUI()
    {
        WindowManagerService.SettingsWindow.Toggle();
    }

    private void OnLogin()
    {
        WindowManagerService.ToolbarWindow.IsOpen = Configuration.OpenOnLogin;
        Configuration.OnLoginUpdates();
        PlayerCharacter.UpdatePlayerCharacter();
    }

    private void OnLogout(int type, int code)
    {
        TabController.ClearAllTabs();
        WindowManagerService.CloseAllWindows();
        PlayerCharacter.UpdatePlayerCharacter();
    }

    #endregion

    public void Dispose()
    {
        ClientState.Login -= OnLogin;
        ClientState.Logout -= OnLogout;

        PluginInterface.UiBuilder.Draw -= OnDrawUI;
        PluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUI;
        PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUI;

        if (MessageService != null)
        {
            ChatGui.ChatMessageUnhandled -= MessageService.HandleChatMessage;
        }

        PluginState?.Dispose();
        MessageService?.Dispose();
        WindowManagerService?.Dispose();
        TabController?.Dispose();

        foreach (string commandAlias in commandAliases)
        {
            if (CommandManager.Commands.Any(t => t.Key == commandAlias))
            {
                CommandManager.RemoveHandler(commandAlias);
            }
        }
    }

    private ChatToolsDbContext InitializeDbContext()
    {
        Logger.Debug("Initializing EF Sqllite Database Context");

        ChatToolsDbContext dbContext = new ChatToolsDbContext(Configuration.MessageDb_FilePath);
        dbContext.Database.EnsureCreated();

        Logger.Debug("EF Sqllite Database Context Initialized");

        return dbContext;
    }

    private void PostDrawEvents()
    {
        TabController.PostDrawEvents();
    }
}
