using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PixelPulse.Database;
using PixelPulse.Models;

namespace PixelPulse;

public class QuoteService
{
    private readonly Random _random = new Random();

    public QuoteService()
    {
        // Initialize the database on service creation
        _ = QuoteDatabase.InitializeAsync();
    }

    public async Task<Quote?> GetRandomQuoteAsync(List<QuoteCategory>? categories = null)
    {
        try
        {
            if (categories != null && categories.Any())
            {
                // Get quotes from any of the specified categories
                var allQuotes = new List<Quote>();
                foreach (var category in categories)
                {
                    var categoryQuotes = await QuoteDatabase.GetQuotesAsync(category);
                    allQuotes.AddRange(categoryQuotes);
                }

                if (!allQuotes.Any())
                    return null;

                return allQuotes[_random.Next(allQuotes.Count)];
            }
            else
            {
                return await QuoteDatabase.GetRandomQuoteAsync();
            }
        }
        catch
        {
            return null;
        }
    }

    // Keep synchronous version for backward compatibility
    public Quote? GetRandomQuote(List<QuoteCategory>? categories = null)
    {
        return GetRandomQuoteAsync(categories).GetAwaiter().GetResult();
    }

    public async Task<Quote?> GetQuoteByCategoryAsync(QuoteCategory category)
    {
        try
        {
            return await QuoteDatabase.GetRandomQuoteAsync(category);
        }
        catch
        {
            return null;
        }
    }

    // Keep synchronous version for backward compatibility
    public Quote? GetQuoteByCategory(QuoteCategory category)
    {
        return GetQuoteByCategoryAsync(category).GetAwaiter().GetResult();
    }

    public async Task<(Quote? Primary, Quote? Secondary)?> GetMatchedQuotesAsync(
        QuoteCategory primaryCategory,
        QuoteCategory secondaryCategory)
    {
        try
        {
            return await QuoteDatabase.GetMatchedQuotesAsync(primaryCategory, secondaryCategory);
        }
        catch
        {
            return null;
        }
    }

    // Keep synchronous version for backward compatibility
    public (Quote? Primary, Quote? Secondary)? GetMatchedQuotes(
        QuoteCategory primaryCategory,
        QuoteCategory secondaryCategory)
    {
        return GetMatchedQuotesAsync(primaryCategory, secondaryCategory).GetAwaiter().GetResult();
    }

    public async Task<(Quote? Primary, Quote? Secondary)?> GetMatchedQuotesAsync(QuoteMatchMode matchMode)
    {
        return matchMode switch
        {
            QuoteMatchMode.MotivationalBible => await GetMatchedQuotesAsync(QuoteCategory.Motivational, QuoteCategory.Bible),
            QuoteMatchMode.LoveMotivational => await GetMatchedQuotesAsync(QuoteCategory.Love, QuoteCategory.Motivational),
            QuoteMatchMode.SuccessLeadership => await GetMatchedQuotesAsync(QuoteCategory.Success, QuoteCategory.Leadership),
            QuoteMatchMode.BusinessSuccess => await GetMatchedQuotesAsync(QuoteCategory.Business, QuoteCategory.Success),
            QuoteMatchMode.WisdomBible => await GetMatchedQuotesAsync(QuoteCategory.Wisdom, QuoteCategory.Bible),
            QuoteMatchMode.EncouragementBible => await GetMatchedQuotesAsync(QuoteCategory.Encouragement, QuoteCategory.Bible),
            QuoteMatchMode.Custom => null, // Will be handled by custom category selection
            _ => null
        };
    }

    // Keep synchronous version for backward compatibility
    public (Quote? Primary, Quote? Secondary)? GetMatchedQuotes(QuoteMatchMode matchMode)
    {
        return GetMatchedQuotesAsync(matchMode).GetAwaiter().GetResult();
    }

    public int GetQuoteCount(QuoteCategory? category = null)
    {
        return QuoteDatabase.GetQuoteCount(category);
    }

    public async Task<List<Quote>> SearchQuotesAsync(string searchTerm)
    {
        try
        {
            return await QuoteDatabase.SearchQuotesAsync(searchTerm);
        }
        catch
        {
            return new List<Quote>();
        }
    }

    public async Task<bool> AddQuoteAsync(Quote quote)
    {
        try
        {
            return await QuoteDatabase.AddQuoteAsync(quote);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateQuoteAsync(Quote quote)
    {
        try
        {
            return await QuoteDatabase.UpdateQuoteAsync(quote);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteQuoteAsync(int quoteId)
    {
        try
        {
            return await QuoteDatabase.DeleteQuoteAsync(quoteId);
        }
        catch
        {
            return false;
        }
    }

    public async Task<DatabaseStats> GetDatabaseStatsAsync()
    {
        try
        {
            return await QuoteDatabase.GetDatabaseStatsAsync();
        }
        catch
        {
            return new DatabaseStats();
        }
    }
}
