using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XIVChatTools.DB;
using XIVChatTools.Helpers;
using XIVChatTools.Services;

namespace XIVChatTools;

public partial class Plugin : IAsyncDalamudPlugin
{
    public static string Name => "Chat Tools";

    [PluginService] private static IChatGui ChatGui { get; set; } = null!;
    [PluginService] private static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] public static IDalamudPluginInterface Interface { get; private set; } = null!;
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
    internal ChatToolsDatabase DbContext { get; private set; } = null!;

    public async Task LoadAsync(CancellationToken token)
    {
        PrintStartupMessage();

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        DbContext = new ChatToolsDatabase(Interface.ConfigDirectory.FullName);

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
    }

    private void PrintStartupMessage()
    {
        string title = "Loading ChatTools";

        if (PluginInterface.IsDev)
        {
            title += " (Dev)";
        }   

        if (PluginInterface.IsTesting)
        {
            title += " (Testing)";
        }

        Logger.Info(title);

#if DEBUG
        if (!PluginInterface.IsDev)
        {
            Logger.Warning("  Debug Build Detected");
        }
#endif
    }

    #region Event Handlers

    private void OnLogin()
    {
        WindowManagerService.MainWindow.IsOpen = Configuration.OpenOnLogin;
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
        if (Plugin.ClientState.IsLoggedIn)
        {
            WindowManagerService.MainWindow.Toggle();
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

    private void PostDrawEvents()
    {
        TabController.PostDrawEvents();
    }
}