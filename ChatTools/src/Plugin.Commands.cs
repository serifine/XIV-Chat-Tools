using Dalamud.Game.Command;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Linq;
using ChatTools.Models;

namespace ChatTools;

public partial class Plugin
{
    private readonly List<CommandDetails> _commands =
    [
        new() { Command = "/chattools", Description = "Open the main Chat Tools window." },
        new() { Command = "/chattools config", Description = "Open the settings menu." },
        new() { Command = "/ctools", Description = "Alias for /chattools." },
        new() { Command = "/ct", Description = "Alias for /chattools.", ShowInHelp = false }
    ];

    private readonly List<string> _settingsArgumentAliases =
    [
        "settings",
        "config"
    ];

    private void SetupCommands()
    {

        foreach (var commandInfo in _commands)
        {
            CommandManager.AddHandler(commandInfo.Command, new CommandInfo(OnCommand)
            {
                ShowInHelp = commandInfo.ShowInHelp,
                HelpMessage = commandInfo.Description,
            });
        }
    }

    private void DisposeCommands()
    {
        foreach (var commandAlias in _commands)
        {
            CommandManager.RemoveHandler(commandAlias.Command);
        }
    }

    private void OnCommand(string command, string args)
    {
        if (_settingsArgumentAliases.Contains(args.ToLower()))
        {
            WindowManagerService.SettingsWindow.IsOpen = !WindowManagerService.SettingsWindow.IsOpen;
        }
        else
        {
            OnOpenMainUI();
        }
    }
}
