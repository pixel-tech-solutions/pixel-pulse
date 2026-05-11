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
        // Self-contained database - no external context needed
    }

    public Quote? GetRandomQuote(List<QuoteCategory>? categories = null)
    {
        try
        {
            var query = _context.Quotes.AsQueryable();

            if (categories != null && categories.Any())
            {
                query = query.Where(q => categories.Contains(q.Category));
            }

            var count = query.Count();
            if (count == 0)
                return null;

            var skip = _random.Next(count);
            return query.Skip(skip).FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    public Quote? GetQuoteByCategory(QuoteCategory category)
    {
        try
        {
            var quotes = _context.Quotes.Where(q => q.Category == category).ToList();
            if (!quotes.Any())
                return null;

            return quotes[_random.Next(quotes.Count)];
        }
        catch
        {
            return null;
        }
    }

    public (Quote? Primary, Quote? Secondary)? GetMatchedQuotes(
        QuoteCategory primaryCategory,
        QuoteCategory secondaryCategory)
    {
        try
        {
            var primary = GetQuoteByCategory(primaryCategory);
            var secondary = GetQuoteByCategory(secondaryCategory);

            if (primary == null || secondary == null)
                return null;

            return (primary, secondary);
        }
        catch
        {
            return null;
        }
    }

    public (Quote? Primary, Quote? Secondary)? GetMatchedQuotes(QuoteMatchMode matchMode)
    {
        return matchMode switch
        {
            QuoteMatchMode.MotivationalBible => GetMatchedQuotes(QuoteCategory.Motivational, QuoteCategory.Bible),
            QuoteMatchMode.LoveMotivational => GetMatchedQuotes(QuoteCategory.Love, QuoteCategory.Motivational),
            QuoteMatchMode.SuccessLeadership => GetMatchedQuotes(QuoteCategory.Success, QuoteCategory.Leadership),
            QuoteMatchMode.BusinessSuccess => GetMatchedQuotes(QuoteCategory.Business, QuoteCategory.Success),
            QuoteMatchMode.WisdomBible => GetMatchedQuotes(QuoteCategory.Wisdom, QuoteCategory.Bible),
            QuoteMatchMode.EncouragementBible => GetMatchedQuotes(QuoteCategory.Encouragement, QuoteCategory.Bible),
            QuoteMatchMode.Custom => null, // Will be handled by custom category selection
            _ => null
        };
    }

    public int GetQuoteCount(QuoteCategory? category = null)
    {
        try
        {
            if (category.HasValue)
                return _context.Quotes.Count(q => q.Category == category.Value);
            return _context.Quotes.Count();
        }
        catch
        {
            return 0;
        }
    }
}
