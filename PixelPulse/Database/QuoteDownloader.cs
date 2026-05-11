using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using PixelPulse.Models;

namespace PixelPulse.Database;

public class QuoteDownloader
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly Dictionary<string, List<string>> _categoryKeywords = new()
    {
        [nameof(QuoteCategory.Motivational).ToLower()] = new() 
        { 
            "motivation", "inspire", "dream", "goal", "success", "achieve", "believe", "courage", 
            "determination", "perseverance", "ambition", "drive", "push", "strive", "work hard",
            "never give up", "keep going", "you can", "make it happen", "possibility", "potential"
        },
        [nameof(QuoteCategory.Love).ToLower()] = new() 
        { 
            "love", "heart", "romance", "relationship", "passion", "affection", "care", "devotion",
            "embrace", "kiss", "intimate", "together", "forever", "soulmate", "adoration", "tenderness"
        },
        [nameof(QuoteCategory.Bible).ToLower()] = new() 
        { 
            "god", "lord", "jesus", "christ", "bible", "scripture", "faith", "pray", "prayer",
            "heaven", "holy", "spirit", "divine", "amen", "bless", "church", "worship", "psalm",
            "proverb", "genesis", "exodus", "matthew", "john", "peter", "paul"
        },
        [nameof(QuoteCategory.Tech).ToLower()] = new() 
        { 
            "technology", "computer", "software", "code", "programming", "digital", "internet",
            "innovation", "invention", "machine", "automation", "artificial", "intelligence", "data",
            "algorithm", "system", "network", "device", "gadget", "electronic", "cyber"
        },
        [nameof(QuoteCategory.Business).ToLower()] = new() 
        { 
            "business", "company", "entrepreneur", "startup", "market", "profit", "revenue",
            "customer", "client", "strategy", "management", "leadership", "team", "organization",
            "investment", "commerce", "trade", "industry", "corporate", "enterprise"
        },
        [nameof(QuoteCategory.Success).ToLower()] = new() 
        { 
            "success", "achievement", "accomplishment", "victory", "triumph", "win", "excel",
            "prosper", "thrive", "flourish", "succeed", "breakthrough", "milestone", "progress",
            "advancement", "growth", "result", "outcome", "performance", "excellence"
        },
        [nameof(QuoteCategory.Leadership).ToLower()] = new() 
        { 
            "lead", "leader", "leadership", "guide", "direct", "manage", "command", "authority",
            "responsibility", "vision", "influence", "empower", "mentor", "coach", "role model",
            "inspire others", "team building", "decision", "strategy", "governance"
        },
        [nameof(QuoteCategory.Wisdom).ToLower()] = new() 
        { 
            "wisdom", "wise", "knowledge", "learning", "understand", "insight", "intelligence",
            "philosophy", "truth", "enlightenment", "awareness", "perception", "judgment",
            "experience", "lesson", "teaching", "guidance", "clarity", "reason", "logic"
        },
        [nameof(QuoteCategory.Encouragement).ToLower()] = new() 
        { 
            "encourage", "support", "uplift", "comfort", "hope", "strength", "confidence",
            "believe in yourself", "you are", "you can", "keep trying", "don't quit", "stay strong",
            "you have", "be brave", "be bold", "be positive", "cheer up", "hang in there"
        },
        [nameof(QuoteCategory.Life).ToLower()] = new() 
        { 
            "life", "living", "existence", "journey", "path", "way", "experience", "moment",
            "time", "days", "years", "birth", "death", "purpose", "meaning", "destiny", "fate",
            "world", "earth", "human", "being", "reality", "existence", "lifetime"
        }
    };

    public static async Task<List<Quote>> DownloadAllQuotesAsync()
    {
        var allQuotes = new List<Quote>();

        try
        {
            Console.WriteLine("Downloading comprehensive quote collection...");

            // Download complete Bible (primary source - ~31,000 verses)
            Console.WriteLine("Downloading complete KJV Bible...");
            var bibleQuotes = await BibleDownloader.DownloadCompleteBibleAsync();
            allQuotes.AddRange(bibleQuotes);
            Console.WriteLine($"Downloaded {bibleQuotes.Count} Bible verses");

            // Download from JamesFT/Database-Quotes-JSON
            Console.WriteLine("Downloading from JamesFT repository...");
            var jamesFTQuotes = await DownloadFromJamesFTAsync();
            allQuotes.AddRange(jamesFTQuotes);
            Console.WriteLine($"Downloaded {jamesFTQuotes.Count} quotes from JamesFT");

            // Download from dwyl/quotes
            Console.WriteLine("Downloading from dwyl repository...");
            var dwylQuotes = await DownloadFromDwylAsync();
            allQuotes.AddRange(dwylQuotes);
            Console.WriteLine($"Downloaded {dwylQuotes.Count} quotes from dwyl");

            // Download from Zen Quotes API (get a sample)
            Console.WriteLine("Downloading from Zen Quotes API...");
            var zenQuotes = await DownloadFromZenQuotesAsync();
            allQuotes.AddRange(zenQuotes);
            Console.WriteLine($"Downloaded {zenQuotes.Count} quotes from Zen Quotes");

            Console.WriteLine($"Total quotes before deduplication: {allQuotes.Count}");

            // Remove duplicates based on text similarity
            var uniqueQuotes = RemoveDuplicates(allQuotes);
            Console.WriteLine($"Unique quotes after deduplication: {uniqueQuotes.Count}");

            // Categorize quotes
            var categorizedQuotes = CategorizeQuotes(uniqueQuotes);

            // Show category breakdown
            var categoryBreakdown = categorizedQuotes
                .GroupBy(q => q.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            Console.WriteLine("Final category breakdown:");
            foreach (var category in categoryBreakdown.OrderByDescending(kv => kv.Value))
            {
                Console.WriteLine($"  {category.Key}: {category.Value} quotes");
            }

            return categorizedQuotes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading quotes: {ex.Message}");
            return allQuotes;
        }
    }

    private static async Task<List<Quote>> DownloadFromJamesFTAsync()
    {
        var quotes = new List<Quote>();
        
        try
        {
            var response = await _httpClient.GetStringAsync("https://raw.githubusercontent.com/JamesFT/Database-Quotes-JSON/master/quotes.json");
            var jsonQuotes = JsonSerializer.Deserialize<List<JamesFTQuote>>(response);

            if (jsonQuotes != null)
            {
                int id = 1;
                foreach (var jsonQuote in jsonQuotes)
                {
                    if (!string.IsNullOrWhiteSpace(jsonQuote.quoteText))
                    {
                        quotes.Add(new Quote
                        {
                            Id = id++,
                            Text = jsonQuote.quoteText.Trim(),
                            Author = string.IsNullOrWhiteSpace(jsonQuote.quoteAuthor) ? "Unknown" : jsonQuote.quoteAuthor.Trim(),
                            Category = QuoteCategory.Motivational, // Will be recategorized later
                            Genre = "inspiration",
                            Source = "JamesFT-Database",
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading from JamesFT: {ex.Message}");
        }

        return quotes;
    }

    private static async Task<List<Quote>> DownloadFromDwylAsync()
    {
        var quotes = new List<Quote>();
        
        try
        {
            var response = await _httpClient.GetStringAsync("https://raw.githubusercontent.com/dwyl/quotes/master/quotes.json");
            var jsonQuotes = JsonSerializer.Deserialize<List<DwylQuote>>(response);

            if (jsonQuotes != null)
            {
                int id = 5000; // Start from a higher ID to avoid conflicts
                foreach (var jsonQuote in jsonQuotes)
                {
                    if (!string.IsNullOrWhiteSpace(jsonQuote.text))
                    {
                        quotes.Add(new Quote
                        {
                            Id = id++,
                            Text = jsonQuote.text.Trim(),
                            Author = string.IsNullOrWhiteSpace(jsonQuote.author) ? "Unknown" : jsonQuote.author.Trim(),
                            Category = QuoteCategory.Motivational, // Will be recategorized later
                            Genre = "inspiration",
                            Source = "Dwyl-Quotes",
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading from dwyl: {ex.Message}");
        }

        return quotes;
    }

    private static async Task<List<Quote>> DownloadFromZenQuotesAsync()
    {
        var quotes = new List<Quote>();
        
        try
        {
            // Get multiple quotes from Zen Quotes API
            for (int i = 0; i < 10; i++) // Get 10 quotes to avoid overwhelming the API
            {
                var response = await _httpClient.GetStringAsync("https://zenquotes.io/api/random");
                var zenQuotes = JsonSerializer.Deserialize<List<ZenQuote>>(response);

                if (zenQuotes != null)
                {
                    foreach (var zenQuote in zenQuotes)
                    {
                        if (!string.IsNullOrWhiteSpace(zenQuote.q))
                        {
                            quotes.Add(new Quote
                            {
                                Id = 10000 + i * 100, // Unique IDs
                                Text = zenQuote.q.Trim(),
                                Author = string.IsNullOrWhiteSpace(zenQuote.a) ? "Unknown" : zenQuote.a.Trim(),
                                Category = QuoteCategory.Motivational, // Zen quotes are typically motivational
                                Genre = "zen",
                                Source = "ZenQuotes-API",
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }
                
                // Small delay to be respectful to the API
                await Task.Delay(100);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading from Zen Quotes: {ex.Message}");
        }

        return quotes;
    }

    private static List<Quote> RemoveDuplicates(List<Quote> quotes)
    {
        var uniqueQuotes = new List<Quote>();
        var seenTexts = new HashSet<string>();

        foreach (var quote in quotes)
        {
            var normalizedText = NormalizeText(quote.Text);
            if (!seenTexts.Contains(normalizedText))
            {
                seenTexts.Add(normalizedText);
                uniqueQuotes.Add(quote);
            }
        }

        return uniqueQuotes;
    }

    private static string NormalizeText(string text)
    {
        // Remove extra whitespace and convert to lowercase for comparison
        return Regex.Replace(text.ToLower().Trim(), @"\s+", " ");
    }

    private static List<Quote> CategorizeQuotes(List<Quote> quotes)
    {
        foreach (var quote in quotes)
        {
            quote.Category = DetermineCategory(quote.Text);
        }
        return quotes;
    }

    private static QuoteCategory DetermineCategory(string quoteText)
    {
        var lowerText = quoteText.ToLower();
        var scores = new Dictionary<QuoteCategory, int>();

        // Score each category based on keyword matches
        foreach (var category in Enum.GetValues<QuoteCategory>())
        {
            var categoryName = category.ToString().ToLower();
            if (_categoryKeywords.ContainsKey(categoryName))
            {
                var keywords = _categoryKeywords[categoryName];
                var score = keywords.Count(keyword => lowerText.Contains(keyword));
                scores[category] = score;
            }
        }

        // Find the category with the highest score
        if (scores.Any())
        {
            var maxScore = scores.Values.Max();
            if (maxScore > 0)
            {
                return scores.First(kvp => kvp.Value == maxScore).Key;
            }
        }

        // Default to Motivational if no clear category found
        return QuoteCategory.Motivational;
    }
}

// JSON models for different quote sources
public class JamesFTQuote
{
    public string quoteText { get; set; } = string.Empty;
    public string quoteAuthor { get; set; } = string.Empty;
}

public class DwylQuote
{
    public string text { get; set; } = string.Empty;
    public string author { get; set; } = string.Empty;
}

public class ZenQuote
{
    public string q { get; set; } = string.Empty; // quote text
    public string a { get; set; } = string.Empty; // author
    public string h { get; set; } = string.Empty; // HTML
}
