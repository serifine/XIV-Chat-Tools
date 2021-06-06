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
    private Configuration _configuration;
    private PluginUI ui;

    // When loaded by LivePluginLoader, the executing assembly will be wrong.
    // Supplying this property allows LivePluginLoader to supply the correct location, so that
    // you have full compatibility when loaded normally and through LPL.
    public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
    private string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
      this._pluginInterface = pluginInterface;

      this._configuration = this._pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      this._configuration.Initialize(this._pluginInterface);

      StateManagementRepository.Init(this._pluginInterface, this._configuration);
      this.stateRepository = StateManagementRepository.Instance;

      this.ui = new PluginUI(this._configuration);

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
      this.ui.Visible = _configuration.OpenOnLogin;
    }

    private void OnLogout(object sender, EventArgs args)
    {
      this.ui.Visible = false;
      this.stateRepository.ClearAllFocusTabs();

      if (this._configuration.PreserveMessagesOnLogout == false)
      {
        this.stateRepository.ClearMessageHistory();
      }
    }

    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString cmessage, ref bool isHandled)
    {
      if (_configuration.DebugLogging && _configuration.DebugLoggingMessages && this._configuration.TrackableChannels.Any(t => t == type))
      {
        PluginLog.Log("NEW CHAT MESSAGE RECEIVED");
        PluginLog.Log("=======================================================");
        PluginLog.Log("Message Type: " + type.ToString());
        PluginLog.Log("Is Marked Handled: " + isHandled.ToString());
        PluginLog.Log("Raw Sender: " + sender.TextValue);
        PluginLog.Log("Parsed Sender: " + ParsePlayerName(type, sender));

        if (_configuration.DebugLoggingMessagePayloads && sender.Payloads.Any())
        {
          PluginLog.Log("");
          PluginLog.Log("SenderPayloads");
          foreach (var payload in sender.Payloads)
          {
            PluginLog.Log("Type: " + payload.Type.ToString());
            PluginLog.Log(payload.ToString());
          }
        }

        if (_configuration.DebugLoggingMessageContents)
        {
          PluginLog.Log("");
          PluginLog.Log("Message: " + cmessage.TextValue);
        }
      }


      if (isHandled || !this._configuration.AllowedChannels.Any(t => t == type))
      {
        return;
      }

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
      if (type == XivChatType.TellIncoming)
      {
        var playerPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

        return (playerPayload as PlayerPayload).PlayerName;
      }

      if (type == XivChatType.TellOutgoing)
      {
        return stateRepository.GetPlayerName();
      }

      if (type == XivChatType.CustomEmote || type == XivChatType.StandardEmote)
      {
        var playerPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

        if (playerPayload != null)
        {
          return (playerPayload as PlayerPayload).PlayerName;
        }

        var textPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.RawText);

        if (textPayload != null)
        {
          return (textPayload as TextPayload).Text;
        }
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

      return "N/A|BadType";
    }
  }
}
