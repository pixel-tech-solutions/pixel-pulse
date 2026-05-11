using System;
using System.Collections.Generic;

namespace PixelPulse.Models
{
    public class UserPreferences
    {
        public int Id { get; set; } = 1;
        
        // Bible version preferences
        public string PreferredBibleVersion { get; set; } = "KJV";
        public List<string> InstalledBibleVersions { get; set; } = new List<string> { "KJV" };
        public bool EnableMultipleVersions { get; set; } = false;
        
        // Content preferences
        public List<QuoteCategory> PreferredCategories { get; set; } = new List<QuoteCategory>();
        public Dictionary<QuoteCategory, int> CategoryWeights { get; set; } = new Dictionary<QuoteCategory, int>();
        
        // User needs assessment
        public UserNeedsProfile NeedsProfile { get; set; } = new UserNeedsProfile();
        
        // Recommendation settings
        public bool EnableRecommendations { get; set; } = true;
        public bool TrackUsagePatterns { get; set; } = true;
        public DateTime LastRecommendationUpdate { get; set; } = DateTime.Now;
        
        // Display preferences
        public bool ShowVerseReferences { get; set; } = true;
        public bool ShowTranslationComparison { get; set; } = false;
        public string DefaultQuoteCategory { get; set; } = "Bible";
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }

    public class UserNeedsProfile
    {
        // Reading preferences
        public string ReadingLevel { get; set; } = "Standard"; // Basic, Standard, Advanced
        public string StudyPurpose { get; set; } = "General"; // Personal, Study, Teaching, Inspiration
        public string TheologicalPerspective { get; set; } = "Traditional"; // Traditional, Contemporary, Literal
        
        // Content focus areas
        public bool FocusOnLoveQuotes { get; set; } = true;
        public bool FocusOnBusinessQuotes { get; set; } = true;
        public bool FocusOnWisdomQuotes { get; set; } = true;
        public bool FocusOnInspirationalQuotes { get; set; } = true;
        
        // Usage patterns
        public int DailyQuoteGoal { get; set; } = 5;
        public string PreferredTimeOfDay { get; set; } = "Morning"; // Morning, Afternoon, Evening
        public bool PreferShortQuotes { get; set; } = false;
        
        // Assessment scores (0-100)
        public int LoveQuotesInterest { get; set; } = 80;
        public int BusinessQuotesInterest { get; set; } = 70;
        public int BibleKnowledgeLevel { get; set; } = 60;
        public int MotivationNeed { get; set; } = 75;
        
        // Demographics (optional)
        public string AgeGroup { get; set; } = "Adult"; // Teen, Young Adult, Adult, Senior
        public string Profession { get; set; } = "General"; // Business, Education, Ministry, Healthcare, etc.
        
        public DateTime LastAssessmentDate { get; set; } = DateTime.Now;
    }

    public enum BibleVersion
    {
        KJV = 1,
        ASV = 2,
        WEB = 3,
        YLT = 4,
        BBE = 5,
        DARBY = 6
    }

    public static class BibleVersionInfo
    {
        public static readonly Dictionary<BibleVersion, string> Names = new()
        {
            { BibleVersion.KJV, "King James Version" },
            { BibleVersion.ASV, "American Standard Version" },
            { BibleVersion.WEB, "World English Bible" },
            { BibleVersion.YLT, "Young's Literal Translation" },
            { BibleVersion.BBE, "Bible in Basic English" },
            { BibleVersion.DARBY, "Darby English Bible" }
        };

        public static readonly Dictionary<BibleVersion, string> Descriptions = new()
        {
            { BibleVersion.KJV, "Traditional, poetic language (1611)" },
            { BibleVersion.ASV, "Modern but formal (1901)" },
            { BibleVersion.WEB, "Contemporary, readable (1997)" },
            { BibleVersion.YLT, "Literal, word-for-word (1862)" },
            { BibleVersion.BBE, "Simplified vocabulary (1949)" },
            { BibleVersion.DARBY, "Study-focused, dispensational (1890)" }
        };

        public static readonly Dictionary<BibleVersion, string> ReadingLevels = new()
        {
            { BibleVersion.KJV, "Advanced" },
            { BibleVersion.ASV, "Standard" },
            { BibleVersion.WEB, "Basic" },
            { BibleVersion.YLT, "Advanced" },
            { BibleVersion.BBE, "Basic" },
            { BibleVersion.DARBY, "Standard" }
        };
    }
}
