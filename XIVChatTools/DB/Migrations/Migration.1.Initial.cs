using Dapper;
using Microsoft.Data.Sqlite;

namespace XIVChatTools.DB.Migrations;

internal class Migration_001_InitialSchema : IMigration
{
    public int Version => 1;

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
    }
}
