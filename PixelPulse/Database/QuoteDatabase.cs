using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PixelPulse.Models;

namespace PixelPulse.Database;

public class QuoteDatabase
{
    private static readonly List<Quote> _quotes = new List<Quote>
    {
        // Inspirational Quotes
        new Quote { Text = "The only way to do great work is to love what you do.", Author = "Steve Jobs", Category = QuoteCategory.Motivational, Genre = "inspiration", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "Innovation distinguishes between a leader and a follower.", Author = "Steve Jobs", Category = QuoteCategory.Leadership, Genre = "leadership", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "The future belongs to those who believe in the beauty of their dreams.", Author = "Eleanor Roosevelt", Category = QuoteCategory.Motivational, Genre = "inspiration", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "Success is not final, failure is not fatal: it is the courage to continue that counts.", Author = "Winston Churchill", Category = QuoteCategory.Success, Genre = "success", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "The best time to plant a tree was 20 years ago. The second best time is now.", Author = "Chinese Proverb", Category = QuoteCategory.Wisdom, Genre = "wisdom", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        
        // Business Quotes
        new Quote { Text = "Your work is going to fill a large part of your life, and the only way to be truly satisfied is to do what you believe is great work.", Author = "Steve Jobs", Category = QuoteCategory.Business, Genre = "business", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "The way to get started is to quit talking and begin doing.", Author = "Walt Disney", Category = QuoteCategory.Business, Genre = "action", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        
        // Technology Quotes
        new Quote { Text = "The advance of technology is based on making it fit in so that you don't really even notice it.", Author = "Bill Gates", Category = QuoteCategory.Tech, Genre = "technology", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "Innovation is the calling card of the future.", Author = "Anna Eshoo", Category = QuoteCategory.Tech, Genre = "innovation", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        
        // Life Quotes
        new Quote { Text = "Life is what happens when you're busy making other plans.", Author = "John Lennon", Category = QuoteCategory.Life, Genre = "life", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "The purpose of our lives is to be happy.", Author = "Dalai Lama", Category = QuoteCategory.Life, Genre = "happiness", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        
        // Encouragement Quotes
        new Quote { Text = "It is during our darkest moments that we must focus to see the light.", Author = "Aristotle", Category = QuoteCategory.Encouragement, Genre = "encouragement", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "Your limitation—it's only your imagination.", Author = "Unknown", Category = QuoteCategory.Encouragement, Genre = "motivation", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        
        // Love Quotes
        new Quote { Text = "Being deeply loved by someone gives you strength, while loving someone deeply gives you courage.", Author = "Lao Tzu", Category = QuoteCategory.Love, Genre = "love", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "Love is patient, love is kind. It does not envy, it does not boast, it is not proud.", Author = "1 Corinthians 13:4", Category = QuoteCategory.Love, Genre = "love", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        
        // Bible Verses
        new Quote { Text = "For God so loved the world that he gave his one and only Son, that whoever believes in him shall not perish but have eternal life.", Author = "John 3:16", Category = QuoteCategory.Bible, Genre = "love", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "I can do all this through him who gives me strength.", Author = "Philippians 4:13", Category = QuoteCategory.Bible, Genre = "faith", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "Trust in the LORD with all your heart and lean not on your own understanding.", Author = "Proverbs 3:5", Category = QuoteCategory.Bible, Genre = "trust", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
        new Quote { Text = "The LORD is my shepherd, I lack nothing.", Author = "Psalm 23:1", Category = QuoteCategory.Bible, Genre = "provision", Source = "PixelPulse", CreatedDate = DateTime.UtcNow },
    };

    public static async Task InitializeAsync()
    {
        // Database is self-contained - no external setup needed
        await Task.CompletedTask;
    }

    public static async Task<List<Quote>> GetQuotesAsync(QuoteCategory? category = null)
    {
        var query = _quotes.AsQueryable();
        
        if (category.HasValue)
        {
            query = query.Where(q => q.Category == category.Value);
        }
        
        return await Task.FromResult(query.ToList());
    }

    public static async Task<Quote?> GetRandomQuoteAsync(QuoteCategory? category = null)
    {
        var query = _quotes.AsQueryable();
        
        if (category.HasValue)
        {
            query = query.Where(q => q.Category == category.Value);
        }
        
        var count = query.Count();
        if (count == 0)
            return null;
            
        var random = new Random();
        var skip = random.Next(count);
        return await Task.FromResult(query.Skip(skip).FirstOrDefault());
    }

    public static async Task<(Quote? Primary, Quote? Secondary)?> GetMatchedQuotesAsync(
        QuoteCategory primaryCategory,
        QuoteCategory secondaryCategory)
    {
        var primary = await GetRandomQuoteAsync(primaryCategory);
        var secondary = await GetRandomQuoteAsync(secondaryCategory);
        
        if (primary == null || secondary == null)
            return null;
            
        return await Task.FromResult((primary, secondary));
    }

    public static int GetQuoteCount(QuoteCategory? category = null)
    {
        var query = _quotes.AsQueryable();
        
        if (category.HasValue)
        {
            query = query.Where(q => q.Category == category.Value);
        }
        
        return query.Count();
    }

    public static List<QuoteCategory> GetAvailableCategories()
    {
        return _quotes.Select(q => q.Category).Distinct().ToList();
    }

    public static async Task<List<Quote>> SearchQuotesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Quote>();
            
        var lowerSearch = searchTerm.ToLower();
        var query = _quotes.AsQueryable()
            .Where(q => q.Text.ToLower().Contains(lowerSearch) || 
                        q.Author.ToLower().Contains(lowerSearch));
        
        return await Task.FromResult(query.ToList());
    }

    public static async Task<bool> AddQuoteAsync(Quote quote)
    {
        if (quote != null && !string.IsNullOrWhiteSpace(quote.Text))
        {
            quote.CreatedDate = DateTime.UtcNow;
            quote.Source = "UserAdded";
            _quotes.Add(quote);
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public static async Task<bool> UpdateQuoteAsync(Quote quote)
    {
        var existing = _quotes.FirstOrDefault(q => q.Id == quote.Id);
        if (existing != null)
        {
            existing.Text = quote.Text;
            existing.Author = quote.Author;
            existing.Category = quote.Category;
            existing.Genre = quote.Genre;
            existing.CreatedDate = DateTime.UtcNow;
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public static async Task<bool> DeleteQuoteAsync(int quoteId)
    {
        var quote = _quotes.FirstOrDefault(q => q.Id == quoteId);
        if (quote != null && quote.Source == "UserAdded")
        {
            _quotes.Remove(quote);
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public static async Task<DatabaseStats> GetDatabaseStatsAsync()
    {
        return await Task.FromResult(new DatabaseStats
        {
            TotalQuotes = _quotes.Count,
            Categories = _quotes.GroupBy(q => q.Category)
                .ToDictionary(g => g.Key, g => g.Count()),
            LastUpdated = _quotes.Max(q => q.CreatedDate),
            Sources = _quotes.GroupBy(q => q.Source)
                .ToDictionary(g => g.Key, g => g.Count())
        });
    }
}

public class DatabaseStats
{
    public int TotalQuotes { get; set; }
    public Dictionary<QuoteCategory, int> Categories { get; set; } = new();
    public Dictionary<string, int> Sources { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
