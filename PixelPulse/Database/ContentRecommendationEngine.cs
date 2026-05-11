using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PixelPulse.Models;

namespace PixelPulse.Database
{
    public class ContentRecommendationEngine
    {
        private static readonly Dictionary<QuoteCategory, List<string>> _loveKeywords = new()
        {
            [QuoteCategory.Love] = new List<string> 
            { 
                "love", "heart", "romance", "relationship", "marriage", "intimacy", "affection", 
                "passion", "devotion", "commitment", "together", "forever", "always",
                "caring", "tenderness", "embrace", "kiss", "hold", "touch", "beloved"
            }
        };

        private static readonly Dictionary<QuoteCategory, List<string>> _businessKeywords = new()
        {
            [QuoteCategory.Business] = new List<string> 
            { 
                "business", "work", "career", "success", "leadership", "management", "strategy",
                "entrepreneur", "innovation", "productivity", "team", "organization", "growth",
                "profit", "investment", "market", "competition", "customer", "service", "quality"
            }
        };

        private static readonly Dictionary<QuoteCategory, List<string>> _wisdomKeywords = new()
        {
            [QuoteCategory.Wisdom] = new List<string> 
            { 
                "wisdom", "knowledge", "understanding", "insight", "learning", "experience",
                "teaching", "guidance", "counsel", "advice", "truth", "principles",
                "discernment", "perception", "clarity", "judgment", "reason", "logic"
            }
        };

        public static async Task<List<Quote>> GetRecommendedQuotesAsync(UserPreferences preferences, int count = 10)
        {
            var recommendedQuotes = new List<Quote>();
            var needs = preferences.NeedsProfile;

            // Calculate category priorities based on user needs
            var categoryScores = CalculateCategoryScores(needs);
            
            // Get quotes from high-priority categories
            var topCategories = categoryScores
                .OrderByDescending(kvp => kvp.Value)
                .Take(3)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var category in topCategories)
            {
                var categoryCount = count / topCategories.Count;
                var categoryQuotes = await GetCategoryQuotesAsync(category, categoryCount, preferences);
                recommendedQuotes.AddRange(categoryQuotes);
            }

            // Add personalized recommendations
            var personalizedQuotes = await GetPersonalizedRecommendationsAsync(needs, count / 4, preferences);
            recommendedQuotes.AddRange(personalizedQuotes);

            // Remove duplicates and return top recommendations
            return recommendedQuotes
                .GroupBy(q => q.Text)
                .Select(g => g.First())
                .Take(count)
                .ToList();
        }

        private static Dictionary<QuoteCategory, double> CalculateCategoryScores(UserNeedsProfile needs)
        {
            var scores = new Dictionary<QuoteCategory, double>();

            // Love quotes scoring
            if (needs.FocusOnLoveQuotes)
            {
                scores[QuoteCategory.Love] = needs.LoveQuotesInterest * 0.8;
                scores[QuoteCategory.Life] = needs.LoveQuotesInterest * 0.4;
                scores[QuoteCategory.Encouragement] = needs.LoveQuotesInterest * 0.3;
            }

            // Business quotes scoring
            if (needs.FocusOnBusinessQuotes)
            {
                scores[QuoteCategory.Business] = needs.BusinessQuotesInterest * 0.8;
                scores[QuoteCategory.Success] = needs.BusinessQuotesInterest * 0.6;
                scores[QuoteCategory.Leadership] = needs.BusinessQuotesInterest * 0.5;
                scores[QuoteCategory.Motivational] = needs.BusinessQuotesInterest * 0.4;
            }

            // Wisdom quotes scoring
            if (needs.FocusOnWisdomQuotes)
            {
                scores[QuoteCategory.Wisdom] = needs.BibleKnowledgeLevel * 0.8;
                scores[QuoteCategory.Life] = needs.BibleKnowledgeLevel * 0.5;
                scores[QuoteCategory.Encouragement] = needs.BibleKnowledgeLevel * 0.3;
            }

            // Motivation scoring
            scores[QuoteCategory.Motivational] = needs.MotivationNeed * 0.7;
            scores[QuoteCategory.Inspirational] = needs.MotivationNeed * 0.6;
            scores[QuoteCategory.Encouragement] = needs.MotivationNeed * 0.5;

            // Age-based adjustments
            scores = AdjustScoresForAge(scores, needs.AgeGroup);

            // Profession-based adjustments
            scores = AdjustScoresForProfession(scores, needs.Profession);

            // Study purpose adjustments
            scores = AdjustScoresForStudyPurpose(scores, needs.StudyPurpose);

            return scores;
        }

        private static Dictionary<QuoteCategory, double> AdjustScoresForAge(Dictionary<QuoteCategory, double> scores, string ageGroup)
        {
            var adjustedScores = new Dictionary<QuoteCategory, double>(scores);

            switch (ageGroup.ToLower())
            {
                case "teen":
                    adjustedScores[QuoteCategory.Motivational] = adjustedScores.GetValueOrDefault(QuoteCategory.Motivational, 0) * 1.3;
                    adjustedScores[QuoteCategory.Life] = adjustedScores.GetValueOrDefault(QuoteCategory.Life, 0) * 1.2;
                    break;
                case "young adult":
                    adjustedScores[QuoteCategory.Business] = adjustedScores.GetValueOrDefault(QuoteCategory.Business, 0) * 1.2;
                    adjustedScores[QuoteCategory.Success] = adjustedScores.GetValueOrDefault(QuoteCategory.Success, 0) * 1.1;
                    break;
                case "adult":
                    adjustedScores[QuoteCategory.Wisdom] = adjustedScores.GetValueOrDefault(QuoteCategory.Wisdom, 0) * 1.2;
                    adjustedScores[QuoteCategory.Leadership] = adjustedScores.GetValueOrDefault(QuoteCategory.Leadership, 0) * 1.1;
                    break;
                case "senior":
                    adjustedScores[QuoteCategory.Life] = adjustedScores.GetValueOrDefault(QuoteCategory.Life, 0) * 1.3;
                    adjustedScores[QuoteCategory.Encouragement] = adjustedScores.GetValueOrDefault(QuoteCategory.Encouragement, 0) * 1.2;
                    break;
            }

            return adjustedScores;
        }

        private static Dictionary<QuoteCategory, double> AdjustScoresForProfession(Dictionary<QuoteCategory, double> scores, string profession)
        {
            var adjustedScores = new Dictionary<QuoteCategory, double>(scores);

            switch (profession.ToLower())
            {
                case "business":
                    adjustedScores[QuoteCategory.Business] = adjustedScores.GetValueOrDefault(QuoteCategory.Business, 0) * 1.5;
                    adjustedScores[QuoteCategory.Leadership] = adjustedScores.GetValueOrDefault(QuoteCategory.Leadership, 0) * 1.3;
                    adjustedScores[QuoteCategory.Success] = adjustedScores.GetValueOrDefault(QuoteCategory.Success, 0) * 1.2;
                    break;
                case "education":
                    adjustedScores[QuoteCategory.Wisdom] = adjustedScores.GetValueOrDefault(QuoteCategory.Wisdom, 0) * 1.4;
                    adjustedScores[QuoteCategory.Encouragement] = adjustedScores.GetValueOrDefault(QuoteCategory.Encouragement, 0) * 1.2;
                    break;
                case "ministry":
                    adjustedScores[QuoteCategory.Bible] = adjustedScores.GetValueOrDefault(QuoteCategory.Bible, 0) * 1.5;
                    adjustedScores[QuoteCategory.Wisdom] = adjustedScores.GetValueOrDefault(QuoteCategory.Wisdom, 0) * 1.3;
                    break;
                case "healthcare":
                    adjustedScores[QuoteCategory.Encouragement] = adjustedScores.GetValueOrDefault(QuoteCategory.Encouragement, 0) * 1.4;
                    adjustedScores[QuoteCategory.Life] = adjustedScores.GetValueOrDefault(QuoteCategory.Life, 0) * 1.2;
                    break;
            }

            return adjustedScores;
        }

        private static Dictionary<QuoteCategory, double> AdjustScoresForStudyPurpose(Dictionary<QuoteCategory, double> scores, string studyPurpose)
        {
            var adjustedScores = new Dictionary<QuoteCategory, double>(scores);

            switch (studyPurpose.ToLower())
            {
                case "personal":
                    adjustedScores[QuoteCategory.Encouragement] = adjustedScores.GetValueOrDefault(QuoteCategory.Encouragement, 0) * 1.3;
                    adjustedScores[QuoteCategory.Motivational] = adjustedScores.GetValueOrDefault(QuoteCategory.Motivational, 0) * 1.2;
                    break;
                case "study":
                    adjustedScores[QuoteCategory.Wisdom] = adjustedScores.GetValueOrDefault(QuoteCategory.Wisdom, 0) * 1.5;
                    adjustedScores[QuoteCategory.Bible] = adjustedScores.GetValueOrDefault(QuoteCategory.Bible, 0) * 1.4;
                    break;
                case "teaching":
                    adjustedScores[QuoteCategory.Wisdom] = adjustedScores.GetValueOrDefault(QuoteCategory.Wisdom, 0) * 1.6;
                    adjustedScores[QuoteCategory.Leadership] = adjustedScores.GetValueOrDefault(QuoteCategory.Leadership, 0) * 1.3;
                    break;
                case "inspiration":
                    adjustedScores[QuoteCategory.Motivational] = adjustedScores.GetValueOrDefault(QuoteCategory.Motivational, 0) * 1.5;
                    adjustedScores[QuoteCategory.Inspirational] = adjustedScores.GetValueOrDefault(QuoteCategory.Inspirational, 0) * 1.4;
                    break;
            }

            return adjustedScores;
        }

        private static async Task<List<Quote>> GetCategoryQuotesAsync(QuoteCategory category, int count, UserPreferences preferences)
        {
            try
            {
                var preferredVersion = BibleVersionManager.GetPreferredVersion();
                return await BibleVersionManager.GetQuotesAsync(preferredVersion, category);
            }
            catch
            {
                // Fallback to default database if version not available
                return await QuoteDatabase.GetQuotesAsync(category);
            }
        }

        private static async Task<List<Quote>> GetPersonalizedRecommendationsAsync(UserNeedsProfile needs, int count, UserPreferences preferences)
        {
            var personalizedQuotes = new List<Quote>();

            // Love-focused recommendations
            if (needs.FocusOnLoveQuotes && needs.LoveQuotesInterest > 70)
            {
                var loveQuotes = await GetQuotesByKeywordsAsync(_loveKeywords[QuoteCategory.Love], count / 2);
                personalizedQuotes.AddRange(loveQuotes);
            }

            // Business-focused recommendations
            if (needs.FocusOnBusinessQuotes && needs.BusinessQuotesInterest > 60)
            {
                var businessQuotes = await GetQuotesByKeywordsAsync(_businessKeywords[QuoteCategory.Business], count / 2);
                personalizedQuotes.AddRange(businessQuotes);
            }

            // Wisdom-focused recommendations
            if (needs.FocusOnWisdomQuotes && needs.BibleKnowledgeLevel > 50)
            {
                var wisdomQuotes = await GetQuotesByKeywordsAsync(_wisdomKeywords[QuoteCategory.Wisdom], count / 2);
                personalizedQuotes.AddRange(wisdomQuotes);
            }

            return personalizedQuotes.Take(count).ToList();
        }

        private static async Task<List<Quote>> GetQuotesByKeywordsAsync(List<string> keywords, int count)
        {
            var allQuotes = new List<Quote>();
            
            // Search across all installed Bible versions
            var preferredVersion = BibleVersionManager.GetPreferredVersion();
            var searchResults = await BibleVersionManager.SearchQuotesAsync(preferredVersion, keywords.First());
            
            // Filter by keyword relevance
            var relevantQuotes = searchResults
                .Where(q => keywords.Any(keyword => 
                    q.Text.ToLower().Contains(keyword.ToLower()) ||
                    q.Category.ToString().ToLower().Contains(keyword.ToLower())))
                .Take(count)
                .ToList();

            return relevantQuotes;
        }

        public static async Task<List<Quote>> GetLoveQuotesAsync(int count = 10, UserPreferences? preferences = null)
        {
            var loveQuotes = new List<Quote>();
            
            // Get quotes from Love category
            var loveCategoryQuotes = await QuoteDatabase.GetQuotesAsync(QuoteCategory.Love);
            loveQuotes.AddRange(loveCategoryQuotes.Take(count / 2));

            // Get Bible verses with love themes
            var preferredVersion = preferences != null ? 
                BibleVersionManager.GetPreferredVersion() : 
                BibleVersion.KJV;

            var bibleVerses = await BibleVersionManager.SearchQuotesAsync(preferredVersion, "love");
            var loveBibleVerses = bibleVerses
                .Where(v => _loveKeywords[QuoteCategory.Love].Any(keyword => 
                    v.Text.ToLower().Contains(keyword.ToLower())))
                .Take(count / 2);

            loveQuotes.AddRange(loveBibleVerses);

            // Get other quotes with love themes
            var otherQuotes = await QuoteDatabase.SearchQuotesAsync("heart");
            loveQuotes.AddRange(otherQuotes.Take(count - loveQuotes.Count));

            return loveQuotes.Take(count).ToList();
        }

        public static async Task<List<Quote>> GetBusinessQuotesAsync(int count = 10, UserPreferences? preferences = null)
        {
            var businessQuotes = new List<Quote>();
            
            // Get quotes from Business category
            var businessCategoryQuotes = await QuoteDatabase.GetQuotesAsync(QuoteCategory.Business);
            businessQuotes.AddRange(businessCategoryQuotes.Take(count / 2));

            // Get quotes from Success and Leadership categories
            var successQuotes = await QuoteDatabase.GetQuotesAsync(QuoteCategory.Success);
            businessQuotes.AddRange(successQuotes.Take(count / 4));

            var leadershipQuotes = await QuoteDatabase.GetQuotesAsync(QuoteCategory.Leadership);
            businessQuotes.AddRange(leadershipQuotes.Take(count / 4));

            // Get Bible verses with business/work themes
            var preferredVersion = preferences != null ? 
                BibleVersionManager.GetPreferredVersion() : 
                BibleVersion.KJV;

            var bibleVerses = await BibleVersionManager.SearchQuotesAsync(preferredVersion, "work");
            var businessBibleVerses = bibleVerses
                .Where(v => _businessKeywords[QuoteCategory.Business].Any(keyword => 
                    v.Text.ToLower().Contains(keyword.ToLower())))
                .Take(count / 4);

            businessQuotes.AddRange(businessBibleVerses);

            // Get other quotes with business themes
            var otherQuotes = await QuoteDatabase.SearchQuotesAsync("success");
            businessQuotes.AddRange(otherQuotes.Take(count - businessQuotes.Count));

            return businessQuotes.Take(count).ToList();
        }

        public static void UpdateUserPreferences(UserPreferences preferences)
        {
            // Analyze usage patterns and update recommendations
            if (preferences.TrackUsagePatterns)
            {
                // This would integrate with usage tracking to improve recommendations
                // Implementation would depend on collecting usage data over time
            }
        }
    }
}
