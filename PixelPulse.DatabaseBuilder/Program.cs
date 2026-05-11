using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PixelPulse.Database;
using PixelPulse.DataIngestion;

namespace PixelPulse.DatabaseBuilder;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length > 0 && (args[0] == "--report" || args[0] == "-report"))
        {
            await ReportQuoteCountsAsync();
            return;
        }

        Console.WriteLine("========================================");
        Console.WriteLine("Pixel Pulse - Database Builder");
        Console.WriteLine("========================================");
        Console.WriteLine();

        try
        {
            // Initialize database
            Console.WriteLine("Initializing database...");
            using var context = new QuoteDbContext();
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("Database initialized.");
            Console.WriteLine();

            int totalImported = 0;

            // Import from Quote Garden
            Console.WriteLine("Starting Quote Garden import...");
            var quoteGardenImporter = new QuoteGardenImporter(context);
            var quoteGardenCount = await quoteGardenImporter.ImportAllQuotesAsync(progress => Console.WriteLine($"  {progress}"));
            totalImported += quoteGardenCount;
            Console.WriteLine();

            // Import Bible verses
            Console.WriteLine("Starting Bible verses import...");
            var bibleImporter = new BibleImporter(context);
            var bibleCount = await bibleImporter.ImportBibleVersesAsync(progress => Console.WriteLine($"  {progress}"));
            totalImported += bibleCount;
            Console.WriteLine();

            // Import from Quotable
            Console.WriteLine("Starting Quotable import...");
            var quotableImporter = new QuotableImporter(context);
            var quotableCount = await quotableImporter.ImportQuotesAsync(progress => Console.WriteLine($"  {progress}"));
            totalImported += quotableCount;
            Console.WriteLine();

            // Import Love quotes
            Console.WriteLine("Starting Love quotes import...");
            var loveQuotesImporter = new LoveQuotesImporter(context);
            var loveCount = await loveQuotesImporter.ImportLoveQuotesAsync(progress => Console.WriteLine($"  {progress}"));
            totalImported += loveCount;
            Console.WriteLine();


            // Final summary
            Console.WriteLine("========================================");
            Console.WriteLine("Import Summary:");
            Console.WriteLine($"  Quote Garden/Zen Quotes: {quoteGardenCount} quotes");
            Console.WriteLine($"  Bible Verses: {bibleCount} verses");
            Console.WriteLine($"  Quotable: {quotableCount} quotes");
            Console.WriteLine($"  Love Quotes: {loveCount} quotes");
            Console.WriteLine($"  Total Imported: {totalImported} quotes");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("Database build complete! Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    static async Task ReportQuoteCountsAsync()
    {
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PixelPulse", "quotes.db");
        Console.WriteLine("========================================");
        Console.WriteLine("Pixel Pulse - Quote database report");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine($"Database: {dbPath}");
        Console.WriteLine($"Exists:   {System.IO.File.Exists(dbPath)}");
        Console.WriteLine();

        if (!System.IO.File.Exists(dbPath))
        {
            Console.WriteLine("No database found. Run the Database Builder without --report to create and populate it.");
            return;
        }

        using var context = new QuoteDbContext();
        var bySource = await context.Quotes
            .GroupBy(q => q.Source)
            .Select(g => new { Source = g.Key, Count = g.Count() })
            .OrderBy(x => x.Source)
            .ToListAsync();
        var total = await context.Quotes.CountAsync();

        Console.WriteLine("Current counts by source:");
        Console.WriteLine("------------------------");
        foreach (var row in bySource)
            Console.WriteLine($"  {row.Source,-15} {row.Count,10:N0}");
        Console.WriteLine("------------------------");
        Console.WriteLine($"  {"Total",-15} {total,10:N0}");
        Console.WriteLine();

        // Expected ranges (from import design / API limits)
        var expected = new[]
        {
            (Source: "ZenQuotes",      Min: 45,   Max: 55,   Description: "Zen Quotes API (~50)"),
            (Source: "BibleInJson",    Min: 335_000, Max: 336_000, Description: "Bible verses (~335,501)"),
            (Source: "Quotable",       Min: 1,    Max: 10_000, Description: "Quotable API (up to ~7,500)"),
            (Source: "LoveQuotes",     Min: 1,    Max: 10_000, Description: "Love quotes (varies by source)"),
        };

        Console.WriteLine("Expected ranges vs actual:");
        Console.WriteLine("-------------------------");
        var missing = false;
        foreach (var exp in expected)
        {
            var actual = bySource.FirstOrDefault(x => x.Source == exp.Source)?.Count ?? 0;
            var ok = actual >= exp.Min && actual <= exp.Max;
            if (!ok && actual < exp.Min) missing = true;
            var status = ok ? "OK" : (actual < exp.Min ? "LOW/MISSING" : "OK");
            Console.WriteLine($"  {exp.Source,-15} expected ~{exp.Min}-{exp.Max,6:N0}  actual: {actual,8:N0}  [{status}]");
        }
        Console.WriteLine("-------------------------");
        if (missing)
            Console.WriteLine();
        Console.WriteLine("If any source shows LOW/MISSING, run the Database Builder (without --report) to (re)import.");
        await Task.CompletedTask;
    }
}
