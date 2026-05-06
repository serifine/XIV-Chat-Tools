using System.Collections.Generic;
using Dapper;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace XIVChatTools.DB.Migrations;

internal class Migration_001_InitialSchema : IMigration
{
    public int Version => 1;

    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    public void Apply(SqliteConnection connection)
    {
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Players (
                Name  TEXT NOT NULL,
                World TEXT NOT NULL,
                PRIMARY KEY (Name, World)
            );

            CREATE TABLE IF NOT EXISTS Messages (
                Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                OwningPlayerName   TEXT    NOT NULL,
                OwningPlayerWorld  TEXT    NOT NULL,
                SenderName         TEXT    NOT NULL,
                SenderWorld        TEXT    NOT NULL,
                Timestamp          TEXT    NOT NULL,
                ChatType           INTEGER NOT NULL,
                MessageContents    TEXT    NOT NULL,
                FOREIGN KEY (OwningPlayerName, OwningPlayerWorld)
                    REFERENCES Players(Name, World)
            );
        ");

        var rows = connection.Query<(int Id, string MessageContents)>(
            "SELECT Id, MessageContents FROM Messages");

        foreach (var (id, text) in rows)
        {
            // Skip rows that are already JSON (produced by the new format)
            if (text.TrimStart().StartsWith("["))
                continue;

            var parts = new List<IMessagePart> { new MessagePart(text) };
            var json = JsonConvert.SerializeObject(parts, _jsonSettings);

            connection.Execute(
                "UPDATE Messages SET MessageContents = @Json WHERE Id = @Id",
                new { Json = json, Id = id });
        }
    }
}
