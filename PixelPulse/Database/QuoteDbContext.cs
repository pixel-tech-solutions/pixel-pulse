using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using PixelPulse.Models;

namespace PixelPulse.Database;

public class QuoteDbContext : DbContext
{
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<QuoteMatch> QuoteMatches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = Path.Combine(appDataPath, "PixelPulse", "quotes.db");
        
        // Ensure directory exists
        var dbDirectory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }
        
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasIndex(q => q.Category);
            entity.HasIndex(q => q.Source);
        });
        
        modelBuilder.Entity<QuoteMatch>(entity =>
        {
            entity.HasIndex(qm => qm.MatchType);
            entity.HasIndex(qm => new { qm.PrimaryCategory, qm.SecondaryCategory });
        });
    }
}
