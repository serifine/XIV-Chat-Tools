using Dalamud.Plugin.Services;
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using XIVChatTools.DB.Migrations;

namespace XIVChatTools.DB;

internal class MigrationRunner
{
    IPluginLog _logger => Plugin.Logger;
    SqliteConnection _connection;

    internal MigrationRunner(SqliteConnection connection)
    {
        _connection = connection;

        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS SchemaVersion (
                Version   INTEGER NOT NULL,
                AppliedAt TEXT    NOT NULL
            );
        ");
    }

    internal void RunMigrations()
    {
        _logger.Verbose("Checking Database Migrations");
        int currentMigrationVersion = GetMigrationVersion();

        var migrations = GetMigrations(currentMigrationVersion);

        if (migrations.Length == 0)
        {
            _logger.Verbose("  No migrations to apply");
            return;
        }

        foreach (var migration in migrations)
        {
            if (migration.Version <= currentMigrationVersion)
            {
                Plugin.Logger.Verbose($" Skipping migration version {migration.Version} as it has already been applied");
                continue;
            }

            try
            {
                Plugin.Logger.Verbose($"  Applying migration version {migration.Version}");

                migration.Apply(_connection);
                SetMigrationVersion(migration.Version);

                Plugin.Logger.Verbose($"  {migration.Version} applied");
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"  {migration.Version} failed: {ex}");
                throw;
            }
        }

        _logger.Info($"Database migrated to version {GetMigrationVersion()}");
    }

    private IMigration[] GetMigrations(int version)
    {
        var migrations = typeof(IMigration).Assembly
            .GetTypes()
            .Where(t => t.IsClass && t.IsAssignableTo(typeof(IMigration)))
            .Select(t => (IMigration)Activator.CreateInstance(t)!)
            .ToArray();

        CheckForDuplicateMigrations(migrations);

        return migrations
            .Where(m => m.Version > version)
            .OrderBy(m => m.Version)
            .ToArray();
    }

    private void CheckForDuplicateMigrations(IMigration[] migrations)
    {
        var duplicates = migrations
            .GroupBy(m => m.Version)
            .Where(g => g.Count() > 1)
            .ToArray();

        if (duplicates.Length > 0)
        {
            var details = duplicates.Select(g => g.Select(m => m.GetType().Name));

            _logger.Fatal($"Duplicate migration versions detected");

            foreach (var duplicate in duplicates)
            {
                foreach (var migration in duplicate)
                {
                    _logger.Fatal($"  {migration.GetType().FullName} => Version = {migration.Version}");
                }
            }

            throw new Exception($"Duplicate migration versions detected.");
        }
    }

    private int GetMigrationVersion()
    {
        return _connection.ExecuteScalar<int>("SELECT COALESCE(MAX(Version), 0) FROM SchemaVersion");
    }

    private void SetMigrationVersion(int version)
    {
        _connection.Execute("INSERT INTO SchemaVersion (Version, AppliedAt) VALUES (@Version, @AppliedAt)",
            new { Version = version, AppliedAt = DateTime.UtcNow });
    }
}
