using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Dalamud.Game.Chat;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;

namespace ChatTools;

public class AdvancedDebugLogger
{
    private readonly Plugin _plugin;

    private string DirectoryPath => Plugin.Interface.ConfigDirectory.FullName;
    private string FileName => "chat-tools-debug-log.json";
    private string FullFilePath => Path.Combine(DirectoryPath, FileName);

    private IPluginLog Logger => Plugin.Logger;

    public AdvancedDebugLogger(Plugin plugin)
    {
        _plugin = plugin;

        EnsureFilePath();
    }

    private void EnsureFilePath()
    {
        if (Directory.Exists(DirectoryPath) == false)
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not create new chat log directory.");
            }
        }

        if (File.Exists(FullFilePath) == false)
        {
            try
            {
                File.WriteAllLines(FullFilePath, ["[", "", "]"]);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not create new debug log file.");
            }
        }
    }

    internal async void AddNewMessage(IChatMessage message, string parsedSenderName)
    {
        try
        {
            var fileResult = File.ReadAllText(FullFilePath);
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true
            };

            List<object> debugList = JsonSerializer.Deserialize<List<object>>(fileResult, options) ?? [];

            debugList.Add(new
            {
                Timestamp = DateTime.UtcNow,
                ChatType = message.LogKind.ToString(),
                TextValue = message.Message.TextValue,
                ParsedSenderName = parsedSenderName,
                SourceKind = message.SourceKind.ToString(),
                TargetKind = message.TargetKind.ToString(),
                Sender = ParseSeStringForLogging(message.Sender),
                Message = ParseSeStringForLogging(message.Message),
                RawSender = message.Sender.ToJson(),
                RawMessage = message.Message.ToJson()
            });

            string serializedOutput = JsonSerializer.Serialize<List<object>>(debugList, options);

            await File.WriteAllTextAsync(FullFilePath, serializedOutput);
        }
        catch (Exception ex)
        {
            Logger.Error("An error has occurred while trying to update chat log history: " + ex.Message);
        }
    }

    private object ParseSeStringForLogging(SeString seString)
    {
        List<object> payloads = new List<object>();

        foreach (var payload in seString.Payloads)
        {
            if (payload.Type == PayloadType.Unknown) {
                continue;
            }

            if (payload.Type == PayloadType.Player)
            {
                var playerPayload = (PlayerPayload)payload;

                payloads.Add(new
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


                payloads.Add(new
                {
                    Type = itemPayload.Type.ToString(),
                    ItemName = itemPayload.DisplayName ?? "Item Null"
                });

                continue;
            }

            if (payload.Type == PayloadType.Icon) {
                var iconPayload = (IconPayload)payload;

                payloads.Add(new
                {
                    Type = iconPayload.Type.ToString(),
                    Icon = iconPayload.Icon.ToString(),
                    IconValue = iconPayload.ToString()
                });

                continue;
            }

            if (payload.Type == PayloadType.RawText) {
                var textPayload = (TextPayload)payload;

                payloads.Add(new
                {
                    Type = textPayload.Type.ToString(),
                    Text = textPayload.Text?.ToString()
                });

                continue;
            }

            payloads.Add(new
            {
                Type = payload.Type.ToString(),
                Text = payload.ToString() ?? "No Value"
            });

            continue;
        }

        return new
        {
            TextValue = seString.TextValue.ToString(), Payloads = payloads
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
    public SeString Target { get; set; } = null!;
    public SeString Message { get; set; } = null!;
}
