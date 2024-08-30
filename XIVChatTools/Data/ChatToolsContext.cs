using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using XIVChatTools.Database.Models;

namespace XIVChatTools.Database;

public class ChatToolsDbContext : DbContext
{
    private string _filePath;
    private string SqlLiteDbPath => Path.Combine(_filePath, "ChatTools.db");

    public DbSet<Player> Players { get; set; }
    public DbSet<Message> Messages { get; set; }

    public ChatToolsDbContext(string filePath)
    {
        _filePath = filePath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.LogTo(message => Plugin.Logger.Verbose(message), Microsoft.Extensions.Logging.LogLevel.Debug);
        optionsBuilder.UseSqlite($"Data Source={SqlLiteDbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>()
            .HasOne(e => e.OwningPlayer)
            .WithMany(e => e.OwnedMessages);
    }

    internal Player GetLoggedInPlayer() {
        if (Plugin.ClientState.IsLoggedIn == false) {
            throw new InvalidOperationException("Must be logged in to access logged in player.");
        }

        var results = this.Players.FirstOrDefault(t => t.Name == Helpers.PlayerCharacter.Name && t.World == Helpers.PlayerCharacter.World);

        if (results != null) {
            return results;
        }

        return new Player() {
            Name = Helpers.PlayerCharacter.Name,
            World = Helpers.PlayerCharacter.World
        };
    }
}
