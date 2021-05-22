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

    private ChatRepository chatRepository;
    private FocusRepository focusRepository;
    private DalamudPluginInterface pi;
    private Configuration configuration;
    private PluginUI ui;

    // When loaded by LivePluginLoader, the executing assembly will be wrong.
    // Supplying this property allows LivePluginLoader to supply the correct location, so that
    // you have full compatibility when loaded normally and through LPL.
    public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
    private string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
      this.pi = pluginInterface;

      ChatRepository.Init(this.pi);
      FocusRepository.Init(this.pi);

      this.chatRepository = ChatRepository.Instance;
      this.focusRepository = FocusRepository.Instance;

      pi.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;
      // pi.Framework.Gui.HoveredItem 

      this.configuration = this.pi.GetPluginConfig() as Configuration ?? new Configuration();
      this.configuration.Initialize(this.pi);

      this.ui = new PluginUI(this.configuration);

      this.pi.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "Opens the Chat Scanner window."
      });

      this.pi.UiBuilder.OnBuildUi += DrawUI;
      this.pi.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();
    }

    public void Dispose()
    {
      this.ui.Dispose();

      this.pi.Framework.Gui.Chat.OnChatMessage -= Chat_OnChatMessage;
      this.pi.CommandManager.RemoveHandler(commandName);
      this.pi.Dispose();
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
      foreach (var t in pi.ClientState.Actors)
      {
        if (!(t is PlayerCharacter pc)) continue;
        if (pc.Name == nameInput) return pc.ActorId;
      }

      return 0;
    }

    private string getSelectedTarget() {
      return pi.ClientState.Targets.CurrentTarget?.Name;
    }


    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString cmessage, ref bool isHandled)
    {
      if (isHandled)
      {
        return;
      }

      if (type != XivChatType.CustomEmote && type != XivChatType.Party && type != XivChatType.Say && type != XivChatType.TellIncoming && type != XivChatType.TellOutgoing)
      {
        return;
      }

      var senderName = "";
      var playerPayload = sender.Payloads.FirstOrDefault(t => t.Type == PayloadType.Player);

      if (playerPayload != null) {
        senderName = (playerPayload as PlayerPayload).PlayerName;
      } 
      
      if (playerPayload == null || type == XivChatType.TellOutgoing) {
        senderName = chatRepository.getPlayerName();
      }

      // foreach( var payload in sender.Payloads) {
      //   PluginLog.Log(payload.ToString() + "|" + payload.Type);
      // }
      PluginLog.Log("type:"+type);
      PluginLog.Log("senderValue:"+sender.TextValue);
      PluginLog.Log("senderParsedValue:"+senderName);
      PluginLog.Log("SenderToJson:"+sender.ToJson());

      chatRepository.addChatLog(new Models.ChatEntry()
      {
        ChatType = type,
        Message = cmessage.TextValue,
        SenderId = senderId,
        SenderName = senderName
      });
    }
  }
}
