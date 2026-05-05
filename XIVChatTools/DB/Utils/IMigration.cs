using Microsoft.Data.Sqlite;

namespace XIVChatTools.DB.Migrations;

/// <summary>
/// Represents a database migration that can be applied to update the database schema or data to a new version.
/// </summary>
internal interface IMigration
{
    /// <summary>
    /// Gets the version number of the migration. This should be incremented for
    /// each new migration to ensure they are applied in the correct order.
    /// 
    /// <para><b> This must be unique. </b></para>
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Applies the migration to the provided database connection. This method should contain the logic to update the database schema or data as needed for this migration version.
    /// </summary>
    /// <param name="connection">The SQLite connection to apply the migration to.</param>
    void Apply(SqliteConnection connection);
}
