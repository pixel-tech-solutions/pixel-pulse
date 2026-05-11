using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PixelPulse.Models;

namespace PixelPulse.Database
{
    public class BibleVersionManager
    {
        private static readonly Dictionary<BibleVersion, string> _databasePaths = new()
        {
            { BibleVersion.KJV, "bible_kjv.db" },
            { BibleVersion.ASV, "bible_asv.db" },
            { BibleVersion.WEB, "bible_web.db" },
            { BibleVersion.YLT, "bible_ylt.db" },
            { BibleVersion.BBE, "bible_bbe.db" },
            { BibleVersion.DARBY, "bible_darby.db" }
        };

        private static UserPreferences _userPreferences;
        private static Dictionary<BibleVersion, QuoteDbContext> _versionContexts = new();

        public static void Initialize(UserPreferences preferences)
        {
            _userPreferences = preferences;
            LoadInstalledVersions();
        }

        public static async Task<List<Quote>> GetQuotesAsync(BibleVersion version, QuoteCategory? category = null)
        {
            var context = GetVersionContext(version);
            
            if (category.HasValue)
            {
                return await context.Quotes
                    .Where(q => q.Category == category.Value)
                    .ToListAsync();
            }
            
            return await context.Quotes.ToListAsync();
        }

        public static async Task<Quote?> GetRandomQuoteAsync(BibleVersion version, QuoteCategory? category = null)
        {
            var context = GetVersionContext(version);
            
            var query = context.Quotes.AsQueryable();
            
            if (category.HasValue)
            {
                query = query.Where(q => q.Category == category.Value);
            }
            
            return await query
                .OrderBy(q => EF.Functions.Random())
                .FirstOrDefaultAsync();
        }

        public static async Task<List<Quote>> SearchQuotesAsync(BibleVersion version, string searchTerm)
        {
            var context = GetVersionContext(version);
            
            return await context.Quotes
                .Where(q => q.Text.Contains(searchTerm) || 
                           q.Author.Contains(searchTerm))
                .ToListAsync();
        }

        public static async Task<List<Quote>> GetVerseComparisonAsync(string reference)
        {
            var comparisonVerses = new List<Quote>();
            
            foreach (var version in _userPreferences.InstalledBibleVersions)
            {
                if (Enum.TryParse<BibleVersion>(version, out var versionEnum))
                {
                    var context = GetVersionContext(versionEnum);
                    var verse = await context.Quotes
                        .FirstOrDefaultAsync(q => q.Author.Contains(reference));
                    
                    if (verse != null)
                    {
                        comparisonVerses.Add(verse);
                    }
                }
            }
            
            return comparisonVerses;
        }

        public static bool IsVersionInstalled(BibleVersion version)
        {
            return _userPreferences.InstalledBibleVersions.Contains(version.ToString());
        }

        public static void InstallVersion(BibleVersion version)
        {
            if (!_userPreferences.InstalledBibleVersions.Contains(version.ToString()))
            {
                _userPreferences.InstalledBibleVersions.Add(version.ToString());
            }
        }

        public static void UninstallVersion(BibleVersion version)
        {
            _userPreferences.InstalledBibleVersions.Remove(version.ToString());
            
            // Close and remove context if open
            if (_versionContexts.ContainsKey(version))
            {
                _versionContexts[version].Dispose();
                _versionContexts.Remove(version);
            }
        }

        private static QuoteDbContext GetVersionContext(BibleVersion version)
        {
            if (_versionContexts.ContainsKey(version))
            {
                return _versionContexts[version];
            }

            var dbPath = GetDatabasePath(version);
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException($"Bible version database not found: {dbPath}");
            }

            var optionsBuilder = new DbContextOptionsBuilder<QuoteDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            
            var context = new QuoteDbContext(optionsBuilder.Options);
            _versionContexts[version] = context;
            
            return context;
        }

        private static string GetDatabasePath(BibleVersion version)
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixelPulse");
            Directory.CreateDirectory(appDataPath);
            
            return Path.Combine(appDataPath, _databasePaths[version]);
        }

        private static void LoadInstalledVersions()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixelPulse");
            Directory.CreateDirectory(appDataPath);
            
            var installedVersions = new List<string>();
            
            foreach (var kvp in _databasePaths)
            {
                var dbPath = Path.Combine(appDataPath, kvp.Value);
                if (File.Exists(dbPath))
                {
                    installedVersions.Add(kvp.Key.ToString());
                }
            }
            
            _userPreferences.InstalledBibleVersions = installedVersions;
        }

        public static async Task<Dictionary<BibleVersion, int>> GetVersionStatisticsAsync()
        {
            var stats = new Dictionary<BibleVersion, int>();
            
            foreach (var versionStr in _userPreferences.InstalledBibleVersions)
            {
                if (Enum.TryParse<BibleVersion>(versionStr, out var version))
                {
                    var context = GetVersionContext(version);
                    var count = await context.Quotes.CountAsync();
                    stats[version] = count;
                }
            }
            
            return stats;
        }

        public static BibleVersion GetPreferredVersion()
        {
            if (Enum.TryParse<BibleVersion>(_userPreferences.PreferredBibleVersion, out var version))
            {
                return version;
            }
            
            return BibleVersion.KJV; // Default fallback
        }

        public static void SetPreferredVersion(BibleVersion version)
        {
            _userPreferences.PreferredBibleVersion = version.ToString();
        }

        public static async Task<List<Quote>> GetRecommendedQuotesAsync(int count = 10)
        {
            var recommendedQuotes = new List<Quote>();
            var preferredVersion = GetPreferredVersion();
            
            // Get quotes from preferred categories and versions
            foreach (var category in _userPreferences.PreferredCategories)
            {
                var categoryQuotes = await GetQuotesAsync(preferredVersion, category);
                recommendedQuotes.AddRange(categoryQuotes.Take(count / _userPreferences.PreferredCategories.Count));
            }
            
            // Weight by user preferences
            var weightedQuotes = recommendedQuotes
                .GroupBy(q => q.Category)
                .SelectMany(g => g.Take(_userPreferences.CategoryWeights.GetValueOrDefault(g.Key, 1)))
                .ToList();
            
            return weightedQuotes.Take(count).ToList();
        }

        public static void Dispose()
        {
            foreach (var context in _versionContexts.Values)
            {
                context.Dispose();
            }
            _versionContexts.Clear();
        }
    }
}
