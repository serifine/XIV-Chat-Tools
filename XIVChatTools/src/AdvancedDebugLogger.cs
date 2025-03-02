using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.IoC;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Game.Text.SeStringHandling;
using XIVChatTools.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace XIVChatTools;

public class AdvancedDebugLogger
{
    private readonly Plugin _plugin;

    private string directoryPath => $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Temp\\Logs\\ChatTools";
    private string fileName => "chat-tools-debug-log.json";
    private string fullFilePath => Path.Combine(directoryPath, fileName);

    private IPluginLog _logger => Plugin.Logger;

    public AdvancedDebugLogger(Plugin plugin)
    {
        _plugin = plugin;

        EnsureFilePath();
    }

    private void EnsureFilePath()
    {
        if (Directory.Exists(directoryPath) == false)
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Could not create new chat log directory.");
            }
        }

        if (File.Exists(fullFilePath) == false)
        {
            try
            {
                File.WriteAllLines(fullFilePath, ["[", "", "]"]);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Could not create new debug log file.");
            }
        }
    }

    internal async void AddNewMessage(AdvancedDebugEntry message)
    {
        try
        {
            var FileResult = File.ReadAllText(fullFilePath);
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true
            };

            List<object> debugList = JsonSerializer.Deserialize<List<object>>(FileResult, options) ?? [];

            debugList.Add(new
            {
                Timestamp = message.Timestamp,
                ChatType = message.ChatType,
                TextValue = message.TextValue,
                ParsedSenderName = message.ParsedSender,
                Sender = ParseSeStringForLogging(message.Sender),
                Message = ParseSeStringForLogging(message.Message)
            });

            string serializedOutput = JsonSerializer.Serialize<List<object>>(debugList, options);

            await File.WriteAllTextAsync(fullFilePath, serializedOutput);
        }
        catch (Exception ex)
        {
            _logger.Error("An error has occurred while trying to update chat log history: " + ex.Message);
        }
    }

    private object ParseSeStringForLogging(SeString seString)
    {
        List<object> Payloads = new List<object>();

        foreach (var payload in seString.Payloads)
        {
            if (payload.Type == PayloadType.Unknown) {
                continue;
            }

            if (payload.Type == PayloadType.Player)
            {
                var playerPayload = (PlayerPayload)payload;

                Payloads.Add(new
                {
                    Type = playerPayload.Type.ToString(),
                    PlayerName = playerPayload.PlayerName.ToString(),
                    WorldName = playerPayload.World.ToString()
                });

                continue;
            }

            if (payload.Type == PayloadType.Item)
            {
                var itemPayload = (ItemPayload)payload;


                Payloads.Add(new
                {
                    Type = itemPayload.Type.ToString(),
                    ItemName = itemPayload.DisplayName ?? "Item Null"
                });

                continue;
            }

            if (payload.Type == PayloadType.Icon) {
                var iconPayload = (IconPayload)payload;

                Payloads.Add(new
                {
                    Type = iconPayload.Type.ToString(),
                    Icon = iconPayload.Icon.ToString(),
                    IconValue = iconPayload.ToString()
                });

                continue;
            }

            if (payload.Type == PayloadType.RawText) {
                var textPayload = (TextPayload)payload;

                Payloads.Add(new
                {
                    Type = textPayload.Type.ToString(),
                    Text = textPayload.Text?.ToString()
                });

                continue;
            }

            Payloads.Add(new
            {
                Type = payload.Type.ToString(),
                Text = payload.ToString() ?? "No Value"
            });

            continue;
        }

        return new
        {
            TextValue = seString.TextValue.ToString(),
            Payloads
        };
    }
}

public class AdvancedDebugEntry
{
    public int Timestamp { get; set; }
    public string ChatType { get; set; } = null!;
    public string TextValue { get; set; } = null!;
    public string ParsedSender { get; set; } = null!;
    public SeString Sender { get; set; } = null!;
    public SeString Message { get; set; } = null!;
}
