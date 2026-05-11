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

public class QuoteGardenImporter
{
    private const string BaseUrl = "https://quote-garden.onrender.com/api/v3";
    private readonly HttpClient _httpClient;
    private readonly QuoteDbContext _context;

    public QuoteGardenImporter(QuoteDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    }

    public async Task<int> ImportAllQuotesAsync(Action<string>? progressCallback = null)
    {
        progressCallback?.Invoke("Quote Garden API is currently suspended. Using alternative sources...");
        
        // Quote Garden API is suspended, so we'll use direct quote endpoints or skip
        // Try to use Zen Quotes API as alternative
        int totalImported = 0;
        
        try
        {
            // Try Zen Quotes API as alternative
            progressCallback?.Invoke("Fetching quotes from Zen Quotes API...");
            var zenQuotes = await GetZenQuotesAsync(50); // Get 50 quotes per request
            progressCallback?.Invoke($"Fetched {zenQuotes.Count} quotes from Zen Quotes");
            
            foreach (var quoteData in zenQuotes)
            {
                var exists = await _context.Quotes
                    .AnyAsync(q => q.Text == quoteData.Text && q.Source == "ZenQuotes");

                if (!exists)
                {
                    var quote = new Quote
                    {
                        Text = quoteData.Text,
                        Author = quoteData.Author,
                        Category = MapTextToCategory(quoteData.Text),
                        Genre = "inspirational",
                        Source = "ZenQuotes",
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Quotes.Add(quote);
                    totalImported++;

                    if (totalImported % 100 == 0)
                    {
                        await _context.SaveChangesAsync();
                        progressCallback?.Invoke($"Imported {totalImported} quotes...");
                    }
                }
            }
            
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            progressCallback?.Invoke($"Error importing from Zen Quotes: {ex.Message}");
        }

        progressCallback?.Invoke($"Quote import complete. Imported {totalImported} new quotes.");
        return totalImported;
    }
    
    private async Task<List<(string Text, string Author)>> GetZenQuotesAsync(int count)
    {
        var quotes = new List<(string, string)>();
        try
        {
            // Zen Quotes API: /api/quotes returns 50 quotes
            var url = "https://zenquotes.io/api/quotes";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<ZenQuote>>(json);
                
                if (result != null)
                {
                    foreach (var q in result.Take(count))
                    {
                        quotes.Add((q.q ?? "", q.a ?? "Unknown"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching Zen Quotes: {ex.Message}");
        }
        return quotes;
    }
    
    private QuoteCategory MapTextToCategory(string text)
    {
        var textLower = text.ToLower();
        if (textLower.Contains("love") || textLower.Contains("heart"))
            return QuoteCategory.Love;
        if (textLower.Contains("success") || textLower.Contains("achieve"))
            return QuoteCategory.Success;
        if (textLower.Contains("business") || textLower.Contains("work"))
            return QuoteCategory.Business;
        if (textLower.Contains("leadership") || textLower.Contains("lead"))
            return QuoteCategory.Leadership;
        if (textLower.Contains("wisdom") || textLower.Contains("wise"))
            return QuoteCategory.Wisdom;
        if (textLower.Contains("encourage") || textLower.Contains("strength"))
            return QuoteCategory.Encouragement;
        return QuoteCategory.Motivational;
    }
    
    private class ZenQuote
    {
        public string? q { get; set; }
        public string? a { get; set; }
    }

    public async Task<List<QuoteGardenQuote>> GetQuotesByGenreAsync(string genre, int limit = 1000)
    {
        var allQuotes = new List<QuoteGardenQuote>();
        int page = 1;
        bool hasMore = true;

        while (hasMore)
        {
            try
            {
                var url = $"{BaseUrl}/quotes?genre={Uri.EscapeDataString(genre)}&limit={limit}&page={page}";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    hasMore = false;
                    break;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<QuoteGardenQuotesResponse>(json);

                if (result?.data != null && result.data.Any())
                {
                    allQuotes.AddRange(result.data);
                    
                    if (result.pagination?.nextPage == null)
                    {
                        hasMore = false;
                    }
                    else
                    {
                        page++;
                    }
                }
                else
                {
                    hasMore = false;
                }

                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching quotes for genre {genre}, page {page}: {ex.Message}");
                hasMore = false;
            }
        }

        return allQuotes;
    }

    private async Task<List<string>> GetGenresAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/genres");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<QuoteGardenGenresResponse>(json);
            
            return result?.data ?? new List<string>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching genres: {ex.Message}");
            return new List<string>();
        }
    }

    private QuoteCategory MapGenreToCategory(string genre)
    {
        var genreLower = genre.ToLower();
        
        if (genreLower.Contains("motivational") || genreLower.Contains("inspirational"))
            return QuoteCategory.Motivational;
        if (genreLower.Contains("love") || genreLower.Contains("romance"))
            return QuoteCategory.Love;
        if (genreLower.Contains("business"))
            return QuoteCategory.Business;
        if (genreLower.Contains("success") || genreLower.Contains("achievement"))
            return QuoteCategory.Success;
        if (genreLower.Contains("leadership") || genreLower.Contains("management"))
            return QuoteCategory.Leadership;
        if (genreLower.Contains("wisdom") || genreLower.Contains("philosophy"))
            return QuoteCategory.Wisdom;
        if (genreLower.Contains("encouragement") || genreLower.Contains("support"))
            return QuoteCategory.Encouragement;
        if (genreLower.Contains("technology") || genreLower.Contains("tech") || genreLower.Contains("innovation"))
            return QuoteCategory.Tech;
        if (genreLower.Contains("life") || genreLower.Contains("general"))
            return QuoteCategory.Life;
        
        return QuoteCategory.Life;
    }

    private class QuoteGardenGenresResponse
    {
        public List<string>? data { get; set; }
    }

    private class QuoteGardenQuotesResponse
    {
        public List<QuoteGardenQuote>? data { get; set; }
        public QuoteGardenPagination? pagination { get; set; }
    }

    private class QuoteGardenPagination
    {
        public int? nextPage { get; set; }
    }

    public class QuoteGardenQuote
    {
        public string quoteText { get; set; } = string.Empty;
        public string quoteAuthor { get; set; } = string.Empty;
        public string quoteGenre { get; set; } = string.Empty;
    }
}
