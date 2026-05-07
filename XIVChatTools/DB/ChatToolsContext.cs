using Dapper;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using XIVChatTools.DB.Migrations;
using XIVChatTools.DB.Models;
using XIVChatTools.Services;

namespace XIVChatTools.DB;

/// <summary>
/// Resolves types from the plugin assembly rather than via Type.GetType(),
/// which fails under Dalamud's custom AssemblyLoadContext.
/// </summary>
internal class PluginSerializationBinder : ISerializationBinder
{
    private static readonly Assembly _assembly = typeof(IMessagePart).Assembly;

    public Type BindToType(string? assemblyName, string typeName)
    {
        return _assembly.GetType(typeName)
            ?? throw new JsonSerializationException($"Cannot resolve type '{typeName}' from plugin assembly.");
    }

    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
        assemblyName = serializedType.Assembly.GetName().Name;
        typeName = serializedType.FullName;
    }
}

internal class MessagePartsTypeHandler : SqlMapper.TypeHandler<List<IMessagePart>>
{
    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto,
        SerializationBinder = new PluginSerializationBinder()
    };

    public override void SetValue(IDbDataParameter parameter, List<IMessagePart>? value)
    {
        parameter.Value = JsonConvert.SerializeObject(value, _jsonSettings);
    }

    public override List<IMessagePart> Parse(object value)
    {
        return JsonConvert.DeserializeObject<List<IMessagePart>>((string)value, _jsonSettings) ?? [];
    }
}

public class ChatToolsDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public ChatToolsDatabase(string filePath)
    {
        string dbPath = Path.Combine(filePath, "ChatTools.db");
        
        SqlMapper.AddTypeHandler(new MessagePartsTypeHandler());

        _connection = new SqliteConnection($"Data Source={dbPath}");
        _connection.Open();

        new MigrationRunner(_connection).RunMigrations();
    }

    internal Player GetLoggedInPlayer()
    {
        if (!Plugin.ClientState.IsLoggedIn)
            throw new InvalidOperationException("Must be logged in to access logged in player.");

        var name = Helpers.PlayerCharacter.Name;
        var world = Helpers.PlayerCharacter.World;

        _connection.Execute(
            "INSERT OR IGNORE INTO Players (Name, World) VALUES (@Name, @World)",
            new { Name = name, World = world });

        return new Player { Name = name, World = world };
    }

    internal void AddMessage(Message message)
    {
        _connection.Execute(@"
            INSERT INTO Messages
                (OwningPlayerName, OwningPlayerWorld, SenderName, SenderWorld, Timestamp, ChatType, MessageContents)
            VALUES
                (@OwningPlayerName, @OwningPlayerWorld, @SenderName, @SenderWorld, @Timestamp, @ChatType, @MessageContents)",
            new
            {
                OwningPlayerName = message.OwningPlayer!.Name,
                OwningPlayerWorld = message.OwningPlayer!.World,
                message.SenderName,
                message.SenderWorld,
                message.Timestamp,
                ChatType = (int)message.ChatType,
                message.MessageContents
            });
    }

    internal IEnumerable<Message> GetAllMessages(string ownerName) => _connection
        .Query<Message>(@"
            SELECT * FROM Messages
            WHERE OwningPlayerName = @Name
            ORDER BY Timestamp",
            new { Name = ownerName });

    internal IEnumerable<Message> GetMessagesForPlayer(string ownerName, string senderName, string senderWorld) => _connection
        .Query<Message>(@"
            SELECT * FROM Messages
            WHERE OwningPlayerName = @OwnerName
              AND SenderName = @SenderName AND SenderWorld = @SenderWorld
              AND Timestamp >= @Cutoff
            ORDER BY Timestamp",
            new { OwnerName = ownerName, SenderName = senderName, SenderWorld = senderWorld, Cutoff = DateTime.Now.AddDays(-14) });

    internal IEnumerable<Message> GetMessagesForPlayers(string ownerName, List<string> playerKeys) => _connection
        .Query<Message>(@"
            SELECT * FROM Messages
            WHERE OwningPlayerName = @OwnerName
              AND (SenderName || '@' || SenderWorld) IN @Keys
              AND Timestamp >= @Cutoff
            ORDER BY Timestamp",
            new { OwnerName = ownerName, Keys = playerKeys, Cutoff = DateTime.Now.AddDays(-14) });

    internal IEnumerable<Message> SearchMessages(string ownerName, string searchText) => _connection
        .Query<Message>(@"
            SELECT * FROM Messages
            WHERE (MessageContents LIKE @Search OR SenderName LIKE @Search)
            ORDER BY Timestamp",
            new { Search = $"%{searchText}%" });

    public void Dispose() => _connection.Dispose();
}
