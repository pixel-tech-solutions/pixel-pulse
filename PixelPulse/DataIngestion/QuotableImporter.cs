using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PixelPulse.Database;
using PixelPulse.Models;

namespace PixelPulse.DataIngestion;

public class QuotableImporter
{
    private const string BaseUrl = "https://api.quotable.io";
    private readonly HttpClient _httpClient;
    private readonly QuoteDbContext _context;

    public QuotableImporter(QuoteDbContext context)
    {
        _context = context;
        var handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        };
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMinutes(5)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PixelPulse/1.0");
    }

    public async Task<int> ImportQuotesAsync(Action<string>? progressCallback = null)
    {
        progressCallback?.Invoke("Importing quotes from Quotable API...");
        
        int imported = 0;
        int page = 1;
        bool hasMore = true;

        while (hasMore && page <= 50) // Limit to 50 pages to avoid infinite loops
        {
            try
            {
                var url = $"{BaseUrl}/quotes?page={page}&limit=150";
                progressCallback?.Invoke($"Fetching Quotable page {page}...");
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    progressCallback?.Invoke($"Quotable API returned status {response.StatusCode}");
                    hasMore = false;
                    break;
                }

                var json = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    progressCallback?.Invoke("Quotable API returned empty response");
                    hasMore = false;
                    break;
                }
                
                var result = JsonConvert.DeserializeObject<QuotableResponse>(json);

                if (result?.results != null && result.results.Any())
                {
                    progressCallback?.Invoke($"Processing {result.results.Count} quotes from page {page}...");
                    
                    foreach (var quoteData in result.results)
                    {
                        if (string.IsNullOrWhiteSpace(quoteData.content))
                            continue;
                            
                        var exists = await _context.Quotes
                            .AnyAsync(q => q.Text == quoteData.content && q.Source == "Quotable");

                        if (!exists)
                        {
                            var category = MapTagsToCategory(quoteData.tags);
                            
                            var quote = new Quote
                            {
                                Text = quoteData.content,
                                Author = quoteData.author ?? "Unknown",
                                Category = category,
                                Genre = string.Join(", ", quoteData.tags?.Select(t => t.name) ?? Array.Empty<string>()),
                                Source = "Quotable",
                                CreatedDate = DateTime.UtcNow
                            };

                            _context.Quotes.Add(quote);
                            imported++;

                            if (imported % 100 == 0)
                            {
                                await _context.SaveChangesAsync();
                                progressCallback?.Invoke($"Imported {imported} quotes from Quotable...");
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    
                    if (result.hasNextPage && page < result.totalPages)
                    {
                        page++;
                        await Task.Delay(1000); // Rate limiting
                    }
                    else
                    {
                        hasMore = false;
                    }
                }
                else
                {
                    progressCallback?.Invoke($"No quotes found on page {page}");
                    hasMore = false;
                }
            }
            catch (Exception ex)
            {
                progressCallback?.Invoke($"Error importing from Quotable, page {page}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error importing from Quotable, page {page}: {ex}");
                hasMore = false;
            }
        }

        await _context.SaveChangesAsync();
        progressCallback?.Invoke($"Quotable import complete. Imported {imported} new quotes.");
        return imported;
    }

    private QuoteCategory MapTagsToCategory(List<QuotableTag>? tags)
    {
        if (tags == null || !tags.Any())
            return QuoteCategory.Life;

        var tagNames = string.Join(" ", tags.Select(t => t.name.ToLower()));

        if (tagNames.Contains("motivational") || tagNames.Contains("inspirational"))
            return QuoteCategory.Motivational;
        if (tagNames.Contains("success") || tagNames.Contains("achievement"))
            return QuoteCategory.Success;
        if (tagNames.Contains("leadership") || tagNames.Contains("management"))
            return QuoteCategory.Leadership;
        if (tagNames.Contains("wisdom") || tagNames.Contains("philosophy"))
            return QuoteCategory.Wisdom;
        if (tagNames.Contains("business") || tagNames.Contains("entrepreneurship"))
            return QuoteCategory.Business;
        if (tagNames.Contains("technology") || tagNames.Contains("tech"))
            return QuoteCategory.Tech;
        if (tagNames.Contains("love") || tagNames.Contains("romance"))
            return QuoteCategory.Love;
        if (tagNames.Contains("encouragement") || tagNames.Contains("support"))
            return QuoteCategory.Encouragement;

        return QuoteCategory.Life;
    }

    private class QuotableResponse
    {
        public int count { get; set; }
        public int totalCount { get; set; }
        public int page { get; set; }
        public int totalPages { get; set; }
        public int lastItemIndex { get; set; }
        public bool hasNextPage { get; set; }
        public List<QuotableQuote>? results { get; set; }
    }

    private class QuotableQuote
    {
        public string _id { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public string author { get; set; } = string.Empty;
        public List<QuotableTag>? tags { get; set; }
    }

    private class QuotableTag
    {
        public string name { get; set; } = string.Empty;
    }
}
