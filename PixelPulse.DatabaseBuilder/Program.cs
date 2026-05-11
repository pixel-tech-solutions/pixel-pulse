using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PixelPulse.Database;
using PixelPulse.Models;

namespace PixelPulse.DatabaseBuilder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            
            Console.WriteLine("Pixel Pulse Comprehensive Quote Database Builder");
            Console.WriteLine("=============================================");
            Console.WriteLine("Building database with complete KJV Bible and external quotes...");
            Console.WriteLine();

            try
            {
                // Get the output directory from command line args or use default
                var outputDir = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "..", "PixelPulse", "Resources");
                
                // Ensure output directory exists
                Directory.CreateDirectory(outputDir);

                Console.WriteLine($"Output directory: {outputDir}");
                Console.WriteLine();

                // Download quotes from external sources (including complete Bible)
                Console.WriteLine("Starting comprehensive quote download...");
                var downloadedQuotes = await QuoteDownloader.DownloadAllQuotesAsync();
                Console.WriteLine($"Download completed: {downloadedQuotes.Count:N0} total quotes");

                // Show detailed category breakdown
                var categoryBreakdown = downloadedQuotes
                    .GroupBy(q => q.Category)
                    .ToDictionary(g => g.Key, g => g.Count());

                Console.WriteLine("\n=== Quote Category Breakdown ===");
                foreach (var category in categoryBreakdown.OrderByDescending(kv => kv.Value))
                {
                    var percentage = (category.Value * 100.0 / downloadedQuotes.Count).ToString("F1");
                    Console.WriteLine($"  {category.Key,-15}: {category.Value,6:N0} quotes ({percentage}%)");
                }

                // Show source breakdown
                var sourceBreakdown = downloadedQuotes
                    .GroupBy(q => q.Source)
                    .ToDictionary(g => g.Key, g => g.Count());

                Console.WriteLine("\n=== Source Breakdown ===");
                foreach (var source in sourceBreakdown.OrderByDescending(kv => kv.Value))
                {
                    var percentage = (source.Value * 100.0 / downloadedQuotes.Count).ToString("F1");
                    Console.WriteLine($"  {source.Key,-20}: {source.Value,6:N0} quotes ({percentage}%)");
                }

                // Create modular Bible databases for selective installation
                Console.WriteLine("\nCreating modular Bible databases...");
                await CreateModularBibleDatabasesAsync(outputDir, downloadedQuotes);

                // Create main quotes database with non-Bible content
                var mainDbPath = Path.Combine(outputDir, "quotes.db");
                await CreateMainQuotesDatabaseAsync(mainDbPath, downloadedQuotes);

                stopwatch.Stop();
                
                Console.WriteLine($"\n=== Database Creation Completed ===");
                Console.WriteLine($"Total quotes: {downloadedQuotes.Count:N0}");
                Console.WriteLine($"Main database file: {mainDbPath}");
                Console.WriteLine($"File size: {new FileInfo(mainDbPath).Length:N0} bytes ({new FileInfo(mainDbPath).Length / 1024.0 / 1024.0:F1} MB)");
                Console.WriteLine($"Build time: {stopwatch.Elapsed.TotalMinutes:F1} minutes");

                Console.WriteLine($"\n✅ Multi-version Bible quote database built successfully!");
                Console.WriteLine("The database now includes:");
                Console.WriteLine("  • Complete KJV Bible (~31,000 verses)");
                Console.WriteLine("  • ASV (American Standard Version)");
                Console.WriteLine("  • WEB (World English Bible)");
                Console.WriteLine("  • YLT (Young's Literal Translation)");
                Console.WriteLine("  • BBE (Bible in Basic English)");
                Console.WriteLine("  • DARBY (Darby English Bible)");
                Console.WriteLine("  • 5,000+ quotes from external repositories");
                Console.WriteLine("  • Motivational, wisdom, and inspirational quotes");
                Console.WriteLine("  • Intelligent categorization and deduplication");
                Console.WriteLine("  • Modular installation support");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }

        private static async Task AddQuotesInBatchesAsync(QuoteDbContext context, List<Quote> quotes)
        {
            const int batchSize = 1000;
            var totalBatches = (quotes.Count + batchSize - 1) / batchSize;
            
            for (int i = 0; i < quotes.Count; i += batchSize)
            {
                var batch = quotes.Skip(i).Take(batchSize).ToList();
                context.Quotes.AddRange(batch);
                
                try
                {
                    await context.SaveChangesAsync();
                    var progress = (i + batch.Count) * 100.0 / quotes.Count;
                    Console.Write($"\rProgress: {progress:F1}% ({i + batch.Count:N0}/{quotes.Count:N0} quotes)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError adding batch {i / batchSize + 1}: {ex.Message}");
                    throw;
                }
            }
            Console.WriteLine(); // New line after progress indicator
        }

        private static async Task VerifyDatabaseAsync(QuoteDbContext context)
        {
            Console.WriteLine("\n=== Database Integrity Verification ===");

            var totalQuotes = await context.Quotes.CountAsync();
            var categories = await context.Quotes
                .GroupBy(q => q.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            Console.WriteLine($"Total quotes: {totalQuotes:N0}");
            Console.WriteLine($"Categories: {categories.Count}");

            // Verify each category has sufficient quotes
            foreach (var cat in categories.OrderByDescending(c => c.Count))
            {
                var status = cat.Count >= 100 ? "✅" : "⚠️";
                Console.WriteLine($"  {status} {cat.Category,-15}: {cat.Count,6:N0} quotes");
            }

            // Test random quote retrieval
            Console.WriteLine("\nTesting random quote retrieval...");
            var randomQuotes = await context.Quotes
                .OrderBy(q => EF.Functions.Random())
                .Take(5)
                .ToListAsync();

            Console.WriteLine("Sample quotes from database:");
            foreach (var quote in randomQuotes)
            {
                var maxLength = 80;
                var displayText = quote.Text.Length > maxLength 
                    ? quote.Text.Substring(0, maxLength) + "..." 
                    : quote.Text;
                Console.WriteLine($"  \"{displayText}\" - {quote.Author} ({quote.Category})");
            }
        }

        private static async Task TestDatabasePerformanceAsync(QuoteDbContext context)
        {
            Console.WriteLine("\n=== Performance Testing ===");

            var stopwatch = Stopwatch.StartNew();

            // Test random quote retrieval
            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                await context.Quotes
                    .OrderBy(q => EF.Functions.Random())
                    .FirstOrDefaultAsync();
            }
            stopwatch.Stop();
            var randomQuoteSpeed = 100.0 / stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine($"Random quote retrieval: {randomQuoteSpeed:F1} quotes/second");

            // Test category filtering
            stopwatch.Restart();
            var bibleQuotes = await context.Quotes
                .Where(q => q.Category == QuoteCategory.Bible)
                .ToListAsync();
            stopwatch.Stop();
            Console.WriteLine($"Bible category filter: {bibleQuotes.Count:N0} quotes in {stopwatch.ElapsedMilliseconds:F0}ms");

            // Test search functionality
            stopwatch.Restart();
            var searchResults = await context.Quotes
                .Where(q => q.Text.Contains("love") || q.Text.Contains("hope"))
                .ToListAsync();
            stopwatch.Stop();
            Console.WriteLine($"Search for 'love' or 'hope': {searchResults.Count:N0} results in {stopwatch.ElapsedMilliseconds:F0}ms");
        }

        private static async Task CreateModularBibleDatabasesAsync(string outputDir, List<Quote> allQuotes)
        {
            var bibleQuotes = allQuotes.Where(q => q.Source.EndsWith("-Bible")).ToList();
            var bibleVersions = bibleQuotes.GroupBy(q => q.Source).ToList();

            Console.WriteLine("Creating separate database files for each Bible version...");

            foreach (var versionGroup in bibleVersions)
            {
                var versionCode = versionGroup.Key.Replace("-Bible", "").ToUpper();
                var dbPath = Path.Combine(outputDir, $"bible_{versionCode.ToLower()}.db");
                
                Console.WriteLine($"Creating {versionGroup.Key} database: {dbPath}");
                
                var optionsBuilder = new DbContextOptionsBuilder<QuoteDbContext>();
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                
                using var context = new QuoteDbContext(optionsBuilder.Options);
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                await AddQuotesInBatchesAsync(context, versionGroup.ToList());
                
                var fileInfo = new FileInfo(dbPath);
                var fileSizeMB = fileInfo.Length / 1024.0 / 1024.0;
                var versionCount = versionGroup.Count();
                Console.WriteLine("  " + versionGroup.Key + ": " + versionCount.ToString("N0") + " verses (" + fileSizeMB.ToString("F1") + " MB)");
            }
        }

        private static async Task CreateMainQuotesDatabaseAsync(string dbPath, List<Quote> allQuotes)
        {
            var nonBibleQuotes = allQuotes.Where(q => !q.Source.EndsWith("-Bible")).ToList();
            
            Console.WriteLine($"Creating main quotes database: {dbPath}");
            
            var optionsBuilder = new DbContextOptionsBuilder<QuoteDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            
            using var context = new QuoteDbContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            await AddQuotesInBatchesAsync(context, nonBibleQuotes);
            
            var fileInfo = new FileInfo(dbPath);
            var fileSizeMB = fileInfo.Length / 1024.0 / 1024.0;
            var quotesCount = nonBibleQuotes.Count;
            Console.WriteLine("  Main Database: " + quotesCount.ToString("N0") + " quotes (" + fileSizeMB.ToString("F1") + " MB)");
        }
    }
}
