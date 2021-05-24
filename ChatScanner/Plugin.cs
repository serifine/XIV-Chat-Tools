using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace ChatScanner
{
  public class Plugin : IDalamudPlugin
  {
    public string Name => "Chat Scanner";

    private const string commandName = "/cScan";


    private StateManagementRepository stateRepository;
    private DalamudPluginInterface _pluginInterface;
    private List<XivChatType> _allowedChannels;
    private Configuration configuration;
    private PluginUI ui;

    // When loaded by LivePluginLoader, the executing assembly will be wrong.
    // Supplying this property allows LivePluginLoader to supply the correct location, so that
    // you have full compatibility when loaded normally and through LPL.
    public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
    private string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
      this._pluginInterface = pluginInterface;

      StateManagementRepository.Init(this._pluginInterface);
      this.stateRepository = StateManagementRepository.Instance;

      this.configuration = this._pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      this.configuration.Initialize(this._pluginInterface);

      this.ui = new PluginUI(this.configuration);

      this._pluginInterface.CommandManager.AddHandler("/chatScanner", new CommandInfo(OnCommand)
      {
        HelpMessage = "Opens the Chat Scanner window."
      });
      this._pluginInterface.CommandManager.AddHandler("/cScanner", new CommandInfo(OnCommand)
      {
        HelpMessage = "Alias for /chatScanner."
      });
      this._pluginInterface.CommandManager.AddHandler("/cscan", new CommandInfo(OnCommand)
      {
        HelpMessage = "Alias for /chatScanner."
      });

      _allowedChannels = configuration.AllowedChannels;

      _pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;
      this._pluginInterface.ClientState.OnLogout += this.OnLogout;
      this._pluginInterface.ClientState.OnLogin += this.OnLogin;

      this._pluginInterface.UiBuilder.OnBuildUi += DrawUI;
      this._pluginInterface.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();
    }

    public void Dispose()
    {
      this.ui.Dispose();

      this._pluginInterface.Framework.Gui.Chat.OnChatMessage -= Chat_OnChatMessage;
      this._pluginInterface.ClientState.OnLogout -= this.OnLogout;
      this._pluginInterface.ClientState.OnLogin -= this.OnLogin;

      this._pluginInterface.CommandManager.RemoveHandler("/chatScanner");
      this._pluginInterface.CommandManager.RemoveHandler("/cScanner");
      this._pluginInterface.CommandManager.RemoveHandler("/cscan");

      this._pluginInterface.Dispose();
      this.stateRepository.Dispose();
    }

    private void OnCommand(string command, string args)
    {
      PluginLog.Log("Command: " + command);
      PluginLog.Log("Args: " + args);
      if (args.ToLower() == "config" || args.ToLower() == "settings")
      {
        this.ui.SettingsVisible = true;
      }
      else
      {
        this.ui.Visible = true;
      }
    }

    private void DrawUI()
    {
      this.ui.Draw();
    }

    private void DrawConfigUI()
    {
      this.ui.SettingsVisible = true;
    }

    private void OnLogin(object sender, EventArgs args)
    {
      this.ui.Visible = configuration.OpenOnLogin;
    }

    private void OnLogout(object sender, EventArgs args)
    {
      this.ui.Visible = false;
      this.stateRepository.ClearAllFocusTabs();

      if (this.configuration.PreserveMessagesOnLogout == false)
      {
        this.stateRepository.ClearMessageHistory();
      }
    }

    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString cmessage, ref bool isHandled)
    {
      if (isHandled || !_allowedChannels.Any(t => t == type))
      {
        return;
      }

      // PluginLog.LogDebug("NEW CHAT MESSAGE RECEIVED");
      // PluginLog.LogDebug("=======================================================");
      // PluginLog.LogDebug("rawSenderValue:" + sender.TextValue);
      // PluginLog.LogDebug("originalParser:" + OldParseName(type, sender));
      // PluginLog.LogDebug("alternateParser:" + ParseName(type, sender));
      // PluginLog.LogDebug("");
      // // PluginLog.LogDebug("SenderToJson:" + sender.ToJson());
      // PluginLog.LogDebug("SenderPayloads");
      // foreach (var payload in sender.Payloads)
      // {
      //   PluginLog.LogDebug("Type" + payload.Type.ToString());
      //   PluginLog.LogDebug(payload.ToString());
      // }
      // PluginLog.LogDebug("");
      // PluginLog.LogDebug("type:" + type);
      // PluginLog.LogDebug("sender:" + ParseName(type, sender));
      // PluginLog.LogDebug("message:" + cmessage.TextValue);
      // PluginLog.LogDebug("");
      // PluginLog.LogDebug("");
      // PluginLog.LogDebug("");
      // PluginLog.LogDebug("");

      stateRepository.AddChatMessage(new Models.ChatEntry()
      {
        ChatType = type,
        Message = cmessage.TextValue,
        SenderId = senderId,
        SenderName = ParsePlayerName(type, sender)
      });
    }

    private string ParsePlayerName(XivChatType type, SeString sender)
    {
      if (type == XivChatType.CustomEmote || type == XivChatType.StandardEmote)
      {
        // enable if custom emotes become hard
        // var playerPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

        // if (playerPayload != null)
        // {
        //   return (playerPayload as PlayerPayload).PlayerName;
        // }

        var textPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.RawText);

        return (textPayload as TextPayload).Text;
      }

      if (type == XivChatType.Party || type == XivChatType.Say)
      {
        var playerPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

        if (playerPayload != null)
        {
          return (playerPayload as PlayerPayload).PlayerName;
        }
        else
        {
          return stateRepository.GetPlayerName();
        }
      }

      if (type == XivChatType.TellIncoming)
      {
        var playerPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

        return (playerPayload as PlayerPayload).PlayerName;
      }

      if (type == XivChatType.TellOutgoing)
      {
        return stateRepository.GetPlayerName();
      }

      return "N/A|BadType";
    }
  }
}
