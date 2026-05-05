using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XIVChatTools.DB.Migrations;
using XIVChatTools.DB.Models;

namespace XIVChatTools.DB;

public class ChatToolsDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public ChatToolsDatabase(string filePath)
    {
        string dbPath = Path.Combine(filePath, "ChatTools.db");
        
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

    internal List<Message> GetAllMessages(string ownerName) =>
        _connection.Query<Message>(@"
            SELECT * FROM Messages
            WHERE OwningPlayerName = @Name
            ORDER BY Timestamp",
            new { Name = ownerName }).AsList();

    internal List<Message> GetMessagesForPlayer(string ownerName, string senderName, string senderWorld) =>
        _connection.Query<Message>(@"
            SELECT * FROM Messages
            WHERE OwningPlayerName = @OwnerName
              AND SenderName = @SenderName AND SenderWorld = @SenderWorld
              AND Timestamp >= @Cutoff
            ORDER BY Timestamp",
            new { OwnerName = ownerName, SenderName = senderName, SenderWorld = senderWorld, Cutoff = DateTime.Now.AddDays(-14) }).AsList();

    internal List<Message> GetMessagesForPlayers(string ownerName, List<string> playerKeys) =>
        _connection.Query<Message>(@"
            SELECT * FROM Messages
            WHERE OwningPlayerName = @OwnerName
              AND (SenderName || '@' || SenderWorld) IN @Keys
              AND Timestamp >= @Cutoff
            ORDER BY Timestamp",
            new { OwnerName = ownerName, Keys = playerKeys, Cutoff = DateTime.Now.AddDays(-14) }).AsList();

    internal List<Message> SearchMessages(string ownerName, string searchText) =>
        _connection.Query<Message>(@"
            SELECT * FROM Messages
            WHERE (MessageContents LIKE @Search OR SenderName LIKE @Search)
            ORDER BY Timestamp",
            new { Search = $"%{searchText}%" }).AsList();

    public void Dispose() => _connection.Dispose();
}
