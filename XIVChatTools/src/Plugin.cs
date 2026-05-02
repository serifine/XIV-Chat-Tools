using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.EntityFrameworkCore;
using XIVChatTools.DB;
using XIVChatTools.Helpers;
using XIVChatTools.Services;

namespace XIVChatTools;
 
public partial class Plugin : IAsyncDalamudPlugin
{
    public static string Name => "Chat Tools";

    [PluginService] private static IChatGui ChatGui { get; set; } = null!;
    [PluginService] private static ICommandManager CommandManager { get; set; } = null!;

    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;

    internal PluginStateService PluginState { get; private set; } = null!;
    internal MessageService MessageService { get; private set; } = null!;
    internal WindowManagerService WindowManagerService { get; private set; } = null!;
    internal TabControllerService TabController { get; private set; } = null!;
    internal Configuration Configuration { get; private set; } = null!;
    internal ChatToolsDbContext DbContext { get; private set; } = null!;

    public Plugin()
    {
#if DEBUG
        Logger.Debug("Chat Tools initialized in debug mode.");
#endif
    }

    public async Task LoadAsync(CancellationToken token)
    {
        Logger.Verbose("Loading Chat Tools");
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        DbContext = await InitializeDbContext();

        Logger.Verbose("Bootstrapping Chat Tools");
        PluginState = new PluginStateService(this);
        MessageService = new MessageService(this);
        TabController = new TabControllerService(this);
        WindowManagerService = new WindowManagerService(this);
        
        ClientState.Login += OnLogin;
        ClientState.Logout += OnLogout;

        PluginInterface.UiBuilder.Draw += OnDrawUI;
        PluginInterface.UiBuilder.OpenMainUi += OnOpenMainUI;
        PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUI;

        ChatGui.ChatMessageUnhandled += MessageService.HandleChatMessage;

        SetupCommands();

        PlayerCharacter.UpdatePlayerCharacter();
        
        Logger.Verbose("Chat Tools Ready!");
        
#if DEBUG
        if (Plugin.ClientState.IsLoggedIn) {
            Logger.Debug("[DEBUG] Opening main window for debug.");
            WindowManagerService.MainWindow.IsOpen = true;
        } else
        {
            Logger.Debug("[DEBUG] Not opening main window on load because player is not logged in.");
        }
#endif
    }

    #region Event Handlers

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

    private void OnDrawUI()
    {
        WindowManagerService.Draw();

        PostDrawEvents();
    }

    private void OnOpenMainUI()
    {
        if (Plugin.ClientState.IsLoggedIn) {
            WindowManagerService.ToolbarWindow.Toggle();
        }
    }

    private void OnOpenConfigUI()
    {
        WindowManagerService.SettingsWindow.Toggle();
    }

    #endregion

    public ValueTask DisposeAsync()
    {
        try
        {
            ClientState.Login -= OnLogin;
            ClientState.Logout -= OnLogout;

            PluginInterface.UiBuilder.Draw -= OnDrawUI;
            PluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUI;
            PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUI;
            ChatGui.ChatMessageUnhandled -= MessageService.HandleChatMessage;

            PluginState.Dispose();
            MessageService.Dispose();
            WindowManagerService.Dispose();
            TabController.Dispose();

            DbContext.Dispose();

            DisposeCommands();

            return ValueTask.CompletedTask;
        }
        catch (Exception exception)
        {
            return ValueTask.FromException(exception);
        }
    }

    private async Task<ChatToolsDbContext> InitializeDbContext()
    {
        Logger.Verbose("Initializing Local SQLite Database Context");

        var dbContext = new ChatToolsDbContext(Configuration.MessageDbFilePath);
        await dbContext.Database.EnsureCreatedAsync();
        
        Logger.Verbose("Warming Up SQLite Context");

        await dbContext.Messages.FirstAsync();

        Logger.Verbose("EF Context Initialized");

        return dbContext;
    }

    private void PostDrawEvents()
    {
        TabController.PostDrawEvents();
    }
}