using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PixelPulse.Database;
using PixelPulse.Models;

namespace PixelPulse.DataIngestion;

public class LoveQuotesImporter
{
    private readonly QuoteDbContext _context;
    private readonly HttpClient _httpClient;

    public LoveQuotesImporter(QuoteDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    }

    public async Task<int> ImportLoveQuotesAsync(Action<string>? progressCallback = null)
    {
        progressCallback?.Invoke("Importing love quotes from various sources...");
        
        int imported = 0;

        // Try to import from GitHub repositories
        var sources = new[]
        {
            "https://raw.githubusercontent.com/JamesFT/Database-Quotes-JSON/master/quotes.json",
            "https://raw.githubusercontent.com/sharmadeepesh/quotes/master/quotes.json",
            "https://raw.githubusercontent.com/Zachatoo/quotes-database/main/quotes.json"
        };

        foreach (var sourceUrl in sources)
        {
            try
            {
                progressCallback?.Invoke($"Fetching from {sourceUrl}...");
                var response = await _httpClient.GetAsync(sourceUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var quotes = ParseJsonQuotes(json);
                    
                    foreach (var quoteData in quotes)
                    {
                        // Check if quote already exists
                        var exists = await _context.Quotes
                            .AnyAsync(q => q.Text == quoteData.Text && 
                                         q.Category == QuoteCategory.Love);

                        if (!exists && !string.IsNullOrWhiteSpace(quoteData.Text))
                        {
                            var quote = new Quote
                            {
                                Text = quoteData.Text,
                                Author = quoteData.Author,
                                Category = QuoteCategory.Love,
                                Genre = "love",
                                Source = "LoveQuotes",
                                CreatedDate = DateTime.UtcNow
                            };

                            _context.Quotes.Add(quote);
                            imported++;

                            if (imported % 50 == 0)
                            {
                                await _context.SaveChangesAsync();
                                progressCallback?.Invoke($"Imported {imported} love quotes...");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing from {sourceUrl}: {ex.Message}");
            }
        }

        // Love quotes from Quote Garden will be imported by QuoteGardenImporter
        // This importer focuses on other sources

        await _context.SaveChangesAsync();
        progressCallback?.Invoke($"Love quotes import complete. Imported {imported} new quotes.");
        return imported;
    }

    private List<(string Text, string? Author)> ParseJsonQuotes(string json)
    {
        var quotes = new List<(string Text, string? Author)>();
        
        try
        {
            // Try to parse as array of quote objects
            var jsonArray = JsonConvert.DeserializeObject<List<dynamic>>(json);
            if (jsonArray != null)
            {
                foreach (var item in jsonArray)
                {
                    var text = item.quote?.ToString() ?? item.text?.ToString() ?? item.Quote?.ToString() ?? item.Text?.ToString() ?? item.q?.ToString();
                    var author = item.author?.ToString() ?? item.Author?.ToString() ?? item.a?.ToString();
                    
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 10) // Filter very short entries
                    {
                        quotes.Add((text ?? "", author));
                    }
                }
            }
            else
            {
                // Try parsing as object with quotes array
                var jsonObj = JsonConvert.DeserializeObject<dynamic>(json);
                if (jsonObj?.quotes != null)
                {
                    var quotesArray = jsonObj.quotes as Newtonsoft.Json.Linq.JArray;
                    if (quotesArray != null)
                    {
                        foreach (var item in quotesArray)
                        {
                            var text = item["quote"]?.ToString() ?? item["text"]?.ToString() ?? item["Quote"]?.ToString() ?? item["Text"]?.ToString();
                            var author = item["author"]?.ToString() ?? item["Author"]?.ToString();
                            
                            if (!string.IsNullOrWhiteSpace(text) && text.Length > 10)
                            {
                                quotes.Add((text, author));
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing JSON quotes: {ex.Message}");
        }

        return quotes;
    }
}
