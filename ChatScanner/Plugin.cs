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

      _pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;

      this._pluginInterface.UiBuilder.OnBuildUi += DrawUI;
      this._pluginInterface.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();
    }

    public void Dispose()
    {
      this.ui.Dispose();

      this._pluginInterface.Framework.Gui.Chat.OnChatMessage -= Chat_OnChatMessage;
      
      this._pluginInterface.CommandManager.RemoveHandler("/chatScanner");
      this._pluginInterface.CommandManager.RemoveHandler("/cScanner");
      this._pluginInterface.CommandManager.RemoveHandler("/cscan");

      this._pluginInterface.Dispose();
      this.stateRepository.Dispose();
    }

    private void OnCommand(string command, string args)
    {
      // in response to the slash command, just display our main ui
      this.ui.Visible = true;
    }

    private void DrawUI()
    {
      this.ui.Draw();
    }

    private void DrawConfigUI()
    {
      this.ui.SettingsVisible = true;
    }

    private int GetActorId(string nameInput)
    {
      foreach (var t in _pluginInterface.ClientState.Actors)
      {
        if (!(t is PlayerCharacter pc)) continue;
        if (pc.Name == nameInput) return pc.ActorId;
      }

      return 0;
    }

    private string getSelectedTarget()
    {
      return _pluginInterface.ClientState.Targets.CurrentTarget?.Name;
    }


    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString cmessage, ref bool isHandled)
    {
      if (isHandled)
      {
        return;
      }

      if (type != XivChatType.StandardEmote && type != XivChatType.CustomEmote && type != XivChatType.Party && type != XivChatType.Say && type != XivChatType.TellIncoming && type != XivChatType.TellOutgoing)
      {
        return;
      }

      var senderName = "";
      var playerPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

      if (playerPayload != null)
      {
        senderName = (playerPayload as PlayerPayload).PlayerName;
      }

      if (playerPayload == null || type == XivChatType.TellOutgoing)
      {
        senderName = stateRepository.getPlayerName();
      }

      // foreach( var payload in sender.Payloads) {
      //   PluginLog.Log(payload.ToString() + "|" + payload.Type);
      // }
      PluginLog.LogDebug("NEW CHAT MESSAGE RECEIVED");
      PluginLog.LogDebug("");
      PluginLog.LogDebug("type:" + type);
      PluginLog.LogDebug("rawSenderValue:" + sender.TextValue);
      PluginLog.LogDebug("senderParsedValue:" + senderName);
      PluginLog.LogDebug("alternateParserResult:" + ParseName(type, sender, cmessage));
      PluginLog.LogDebug("");
      PluginLog.LogDebug("SenderToJson:" + sender.ToJson());
      PluginLog.LogDebug("");
      PluginLog.LogDebug("");
      PluginLog.LogDebug("");

      stateRepository.addChatLog(new Models.ChatEntry()
      {
        ChatType = type,
        Message = cmessage.TextValue,
        SenderId = senderId,
        SenderName = senderName
      });
    }

    private string ParseName(XivChatType type, SeString sender, SeString cmessage)
    {
      var skip = 0;
      if (cmessage.Payloads[0].Type == PayloadType.UIForeground && cmessage.Payloads[1].Type == PayloadType.UIForeground)
      {
        skip += 2;
      }
      var pName = _pluginInterface.ClientState.LocalPlayer.Name;

      if (sender.Payloads[0 + skip].Type == PayloadType.Player)
      {
        var pPayload = (PlayerPayload)sender.Payloads[0 + skip];
        pName = pPayload.PlayerName;
      }

      if (sender.Payloads[0 + skip].Type == PayloadType.Icon && sender.Payloads[1].Type == PayloadType.Player)
      {
        var pPayload = (PlayerPayload)sender.Payloads[1];
        pName = pPayload.PlayerName;
      }

      if (type == XivChatType.StandardEmote || type == XivChatType.CustomEmote)
      {
        if (cmessage.Payloads[0 + skip].Type == PayloadType.Player)
        {
          var pPayload = (PlayerPayload)cmessage.Payloads[0 + skip];
          pName = pPayload.PlayerName;
        }
      }

      return pName;
    }
  }
}
