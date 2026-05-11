using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PixelPulse.Models;

namespace PixelPulse.Database;

public class QuoteDatabase
{
    private static List<Quote>? _bundledQuotes;
    private static QuoteDbContext? _context;
    private static readonly object _lock = new object();

    public static async Task InitializeAsync()
    {
        lock (_lock)
        {
            if (_context == null)
            {
                _context = new QuoteDbContext();
            }
        }

        // Try to load from database first, fall back to embedded quotes
        await LoadQuotesFromDatabaseAsync();
    }

    private static async Task LoadQuotesFromDatabaseAsync()
    {
        try
        {
            if (_context != null)
            {
                var dbQuotes = await _context.Quotes.ToListAsync();
                if (dbQuotes.Any())
                {
                    _bundledQuotes = dbQuotes;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            // Database not available or corrupted, fall back to embedded quotes
            System.Diagnostics.Debug.WriteLine($"Database loading failed: {ex.Message}");
        }

        // Fall back to embedded quotes
        _bundledQuotes = GetEmbeddedQuotes();
    }

    private static List<Quote> GetEmbeddedQuotes()
    {
        var quotes = new List<Quote>();
        int id = 1;

        // Motivational Quotes (Expanded)
        quotes.Add(new Quote { Id = id++, Text = "The only way to do great work is to love what you do.", Author = "Steve Jobs", Category = QuoteCategory.Motivational, Genre = "inspiration", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The future belongs to those who believe in the beauty of their dreams.", Author = "Eleanor Roosevelt", Category = QuoteCategory.Motivational, Genre = "inspiration", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "It is during our darkest moments that we must focus to see the light.", Author = "Aristotle", Category = QuoteCategory.Motivational, Genre = "encouragement", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Your limitation—it's only your imagination.", Author = "Unknown", Category = QuoteCategory.Motivational, Genre = "motivation", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The best time to plant a tree was 20 years ago. The second best time is now.", Author = "Chinese Proverb", Category = QuoteCategory.Motivational, Genre = "wisdom", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Don't watch the clock; do what it does. Keep going.", Author = "Sam Levenson", Category = QuoteCategory.Motivational, Genre = "perseverance", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "A year from now you may wish you had started today.", Author = "Karen Lamb", Category = QuoteCategory.Motivational, Genre = "action", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The harder you work for something, the greater you'll feel when you achieve it.", Author = "Unknown", Category = QuoteCategory.Motivational, Genre = "effort", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        // Leadership Quotes
        quotes.Add(new Quote { Id = id++, Text = "Innovation distinguishes between a leader and a follower.", Author = "Steve Jobs", Category = QuoteCategory.Leadership, Genre = "leadership", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Leadership is not about being in charge. It's about taking care of those in your charge.", Author = "Simon Sinek", Category = QuoteCategory.Leadership, Genre = "responsibility", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The greatest leader is not necessarily the one who does the greatest things.", Author = "Ronald Reagan", Category = QuoteCategory.Leadership, Genre = "influence", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "A leader is one who knows the way, goes the way, and shows the way.", Author = "John C. Maxwell", Category = QuoteCategory.Leadership, Genre = "guidance", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        // Success Quotes
        quotes.Add(new Quote { Id = id++, Text = "Success is not final, failure is not fatal: it is the courage to continue that counts.", Author = "Winston Churchill", Category = QuoteCategory.Success, Genre = "success", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Success usually comes to those who are too busy to be looking for it.", Author = "Henry David Thoreau", Category = QuoteCategory.Success, Genre = "achievement", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The road to success and the road to failure are almost exactly the same.", Author = "Colin R. Davis", Category = QuoteCategory.Success, Genre = "journey", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Success is walking from failure to failure with no loss of enthusiasm.", Author = "Winston Churchill", Category = QuoteCategory.Success, Genre = "persistence", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        // Business Quotes
        quotes.Add(new Quote { Id = id++, Text = "Your work is going to fill a large part of your life, and the only way to be truly satisfied is to do what you believe is great work.", Author = "Steve Jobs", Category = QuoteCategory.Business, Genre = "business", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The way to get started is to quit talking and begin doing.", Author = "Walt Disney", Category = QuoteCategory.Business, Genre = "action", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "In business, the competition will bite you if you keep running; if you stand still, they will swallow you.", Author = "William S. Knudsen", Category = QuoteCategory.Business, Genre = "competition", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The golden rule for every business man is this: Put yourself in your customer's place.", Author = "Orison Swett Marden", Category = QuoteCategory.Business, Genre = "customer", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        // Technology Quotes
        quotes.Add(new Quote { Id = id++, Text = "The advance of technology is based on making it fit in so that you don't really even notice it.", Author = "Bill Gates", Category = QuoteCategory.Tech, Genre = "technology", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Innovation is the calling card of the future.", Author = "Anna Eshoo", Category = QuoteCategory.Tech, Genre = "innovation", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Technology is nothing. What's important is that you have a faith in people, that they're basically good and smart, and if you give them tools, they'll do wonderful things with them.", Author = "Steve Jobs", Category = QuoteCategory.Tech, Genre = "innovation", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The technology you use impresses no one. The experience you create with it is everything.", Author = "Sean Gerety", Category = QuoteCategory.Tech, Genre = "user-experience", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        // Wisdom Quotes
        quotes.Add(new Quote { Id = id++, Text = "The best time to plant a tree was 20 years ago. The second best time is now.", Author = "Chinese Proverb", Category = QuoteCategory.Wisdom, Genre = "wisdom", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The only true wisdom is in knowing you know nothing.", Author = "Socrates", Category = QuoteCategory.Wisdom, Genre = "philosophy", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "In the end, it's not the years in your life that count. It's the life in your years.", Author = "Abraham Lincoln", Category = QuoteCategory.Wisdom, Genre = "life", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The journey of a thousand miles begins with one step.", Author = "Lao Tzu", Category = QuoteCategory.Wisdom, Genre = "journey", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        // Life Quotes
        quotes.Add(new Quote { Id = id++, Text = "Life is what happens when you're busy making other plans.", Author = "John Lennon", Category = QuoteCategory.Life, Genre = "life", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The purpose of our lives is to be happy.", Author = "Dalai Lama", Category = QuoteCategory.Life, Genre = "happiness", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Life is 10% what happens to you and 90% how you react to it.", Author = "Charles R. Swindoll", Category = QuoteCategory.Life, Genre = "attitude", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Life is either a daring adventure or nothing at all.", Author = "Helen Keller", Category = QuoteCategory.Life, Genre = "adventure", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        // Encouragement Quotes
        quotes.Add(new Quote { Id = id++, Text = "It is during our darkest moments that we must focus to see the light.", Author = "Aristotle", Category = QuoteCategory.Encouragement, Genre = "encouragement", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Your limitation—it's only your imagination.", Author = "Unknown", Category = QuoteCategory.Encouragement, Genre = "motivation", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "When you can't look on the bright side, I will sit with you in the dark.", Author = "Alice in Wonderland", Category = QuoteCategory.Encouragement, Genre = "support", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "You are braver than you believe, stronger than you seem, and smarter than you think.", Author = "A.A. Milne", Category = QuoteCategory.Encouragement, Genre = "confidence", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        // Love Quotes
        quotes.Add(new Quote { Id = id++, Text = "Being deeply loved by someone gives you strength, while loving someone deeply gives you courage.", Author = "Lao Tzu", Category = QuoteCategory.Love, Genre = "love", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Love is patient, love is kind. It does not envy, it does not boast, it is not proud.", Author = "1 Corinthians 13:4", Category = QuoteCategory.Love, Genre = "love", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The best thing to hold onto in life is each other.", Author = "Audrey Hepburn", Category = QuoteCategory.Love, Genre = "relationship", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "We are most alive when we're in love.", Author = "John Updike", Category = QuoteCategory.Love, Genre = "passion", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        // Bible Verses
        quotes.Add(new Quote { Id = id++, Text = "For God so loved the world that he gave his one and only Son, that whoever believes in him shall not perish but have eternal life.", Author = "John 3:16", Category = QuoteCategory.Bible, Genre = "love", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "I can do all this through him who gives me strength.", Author = "Philippians 4:13", Category = QuoteCategory.Bible, Genre = "faith", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Trust in the LORD with all your heart and lean not on your own understanding.", Author = "Proverbs 3:5", Category = QuoteCategory.Bible, Genre = "trust", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "The LORD is my shepherd, I lack nothing.", Author = "Psalm 23:1", Category = QuoteCategory.Bible, Genre = "provision", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "Be strong and courageous. Do not be afraid; do not be discouraged.", Author = "Joshua 1:9", Category = QuoteCategory.Bible, Genre = "courage", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });
        quotes.Add(new Quote { Id = id++, Text = "For I know the plans I have for you, declares the LORD.", Author = "Jeremiah 29:11", Category = QuoteCategory.Bible, Genre = "hope", Source = "PixelPulse", CreatedDate = DateTime.UtcNow });

        return quotes;
    }

    public static async Task<List<Quote>> GetQuotesAsync(QuoteCategory? category = null)
    {
        await EnsureInitializedAsync();
        
        var query = _bundledQuotes?.AsQueryable() ?? new List<Quote>().AsQueryable();
        
        if (category.HasValue)
        {
            query = query.Where(q => q.Category == category.Value);
        }
        
        return await Task.FromResult(query.ToList());
    }

    public static async Task<Quote?> GetRandomQuoteAsync(QuoteCategory? category = null)
    {
        await EnsureInitializedAsync();
        
        if (_bundledQuotes == null || !_bundledQuotes.Any())
            return null;

        var query = _bundledQuotes.AsQueryable();
        
        if (category.HasValue)
        {
            query = query.Where(q => q.Category == category.Value);
        }
        
        var quotes = query.ToList();
        if (!quotes.Any())
            return null;
            
        var random = new Random();
        return quotes[random.Next(quotes.Count)];
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
        if (_bundledQuotes == null)
            return 0;

        var query = _bundledQuotes.AsQueryable();
        
        if (category.HasValue)
        {
            query = query.Where(q => q.Category == category.Value);
        }
        
        return query.Count();
    }

    public static List<QuoteCategory> GetAvailableCategories()
    {
        if (_bundledQuotes == null)
            return new List<QuoteCategory>();

        return _bundledQuotes.Select(q => q.Category).Distinct().ToList();
    }

    public static async Task<List<Quote>> SearchQuotesAsync(string searchTerm)
    {
        await EnsureInitializedAsync();
        
        if (string.IsNullOrWhiteSpace(searchTerm) || _bundledQuotes == null)
            return new List<Quote>();
            
        var lowerSearch = searchTerm.ToLower();
        var query = _bundledQuotes.AsQueryable()
            .Where(q => q.Text.ToLower().Contains(lowerSearch) || 
                        q.Author.ToLower().Contains(lowerSearch));
        
        return await Task.FromResult(query.ToList());
    }

    public static async Task<bool> AddQuoteAsync(Quote quote)
    {
        await EnsureInitializedAsync();
        
        if (quote != null && !string.IsNullOrWhiteSpace(quote.Text) && _bundledQuotes != null)
        {
            quote.Id = _bundledQuotes.Any() ? _bundledQuotes.Max(q => q.Id) + 1 : 1;
            quote.CreatedDate = DateTime.UtcNow;
            quote.Source = "UserAdded";
            _bundledQuotes.Add(quote);
            
            // Also add to database if available
            try
            {
                if (_context != null)
                {
                    await _context.Quotes.AddAsync(quote);
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                // Database operation failed, but quote is in memory
            }
            
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public static async Task<bool> UpdateQuoteAsync(Quote quote)
    {
        await EnsureInitializedAsync();
        
        if (_bundledQuotes == null)
            return false;

        var existing = _bundledQuotes.FirstOrDefault(q => q.Id == quote.Id);
        if (existing != null)
        {
            existing.Text = quote.Text;
            existing.Author = quote.Author;
            existing.Category = quote.Category;
            existing.Genre = quote.Genre;
            existing.CreatedDate = DateTime.UtcNow;
            
            // Also update in database if available
            try
            {
                if (_context != null)
                {
                    var dbQuote = await _context.Quotes.FindAsync(quote.Id);
                    if (dbQuote != null)
                    {
                        dbQuote.Text = quote.Text;
                        dbQuote.Author = quote.Author;
                        dbQuote.Category = quote.Category;
                        dbQuote.Genre = quote.Genre;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                // Database operation failed, but quote is updated in memory
            }
            
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public static async Task<bool> DeleteQuoteAsync(int quoteId)
    {
        await EnsureInitializedAsync();
        
        if (_bundledQuotes == null)
            return false;

        var quote = _bundledQuotes.FirstOrDefault(q => q.Id == quoteId);
        if (quote != null && quote.Source == "UserAdded")
        {
            _bundledQuotes.Remove(quote);
            
            // Also remove from database if available
            try
            {
                if (_context != null)
                {
                    var dbQuote = await _context.Quotes.FindAsync(quoteId);
                    if (dbQuote != null)
                    {
                        _context.Quotes.Remove(dbQuote);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                // Database operation failed, but quote is removed from memory
            }
            
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public static async Task<DatabaseStats> GetDatabaseStatsAsync()
    {
        await EnsureInitializedAsync();
        
        if (_bundledQuotes == null)
            return new DatabaseStats();

        return await Task.FromResult(new DatabaseStats
        {
            TotalQuotes = _bundledQuotes.Count,
            Categories = _bundledQuotes.GroupBy(q => q.Category)
                .ToDictionary(g => g.Key, g => g.Count()),
            LastUpdated = _bundledQuotes.Max(q => q.CreatedDate),
            Sources = _bundledQuotes.GroupBy(q => q.Source)
                .ToDictionary(g => g.Key, g => g.Count())
        });
    }

    private static async Task EnsureInitializedAsync()
    {
        if (_bundledQuotes == null)
        {
            await InitializeAsync();
        }
    }
}

public class DatabaseStats
{
    public int TotalQuotes { get; set; }
    public Dictionary<QuoteCategory, int> Categories { get; set; } = new();
    public Dictionary<string, int> Sources { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
