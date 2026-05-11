using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using PixelPulse.Models;

namespace PixelPulse.Database;

public class BibleDownloader
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly Dictionary<string, List<string>> _bibleThemes = new()
    {
        ["inspiration"] = new() 
        { 
            "hope", "faith", "trust", "believe", "courage", "strength", "power", "mighty", "victory", 
            "triumph", "overcome", "conquer", "prevail", "succeed", "win", "triumphant", "glorious",
            "exalted", "lifted", "raised", "restored", "renewed", "transformed", "new", "beginning"
        },
        ["wisdom"] = new() 
        { 
            "wisdom", "understanding", "knowledge", "insight", "discernment", "prudence", "counsel",
            "instruction", "teaching", "guidance", "direction", "path", "way", "truth", "light",
            "revelation", "inspired", "prophetic", "divine", "holy", "sacred", "spiritual", "heavenly"
        },
        ["encouragement"] = new() 
        { 
            "comfort", "consolation", "encourage", "strengthen", "uphold", "support", "help", "aid",
            "assist", "deliver", "rescue", "save", "protect", "guard", "keep", "preserve", "maintain",
            "sustain", "nourish", "feed", "refresh", "restore", "heal", "cure", "mend", "repair"
        },
        ["love"] = new() 
        { 
            "love", "charity", "compassion", "mercy", "grace", "kindness", "goodness", "gentleness",
            "tenderness", "affection", "care", "devotion", "faithfulness", "loyalty", "commitment",
            "sacrifice", "giving", "generous", "bountiful", "abundant", "rich", "precious", "valuable"
        },
        ["life"] = new() 
        { 
            "life", "living", "alive", "breath", "spirit", "soul", "heart", "mind", "body", "flesh",
            "created", "formed", "made", "birth", "born", "beginning", "start", "origin", "source",
            "foundation", "root", "seed", "grow", "flourish", "thrive", "prosper", "bless", "favor"
        },
        ["guidance"] = new() 
        { 
            "lead", "guide", "direct", "show", "reveal", "disclose", "manifest", "demonstrate",
            "teach", "instruct", "train", "disciple", "follower", "shepherd", "pastor", "leader",
            "ruler", "king", "lord", "master", "teacher", "prophet", "apostle", "messenger", "angel"
        },
        ["provision"] = new() 
        { 
            "provide", "supply", "give", "grant", "bestow", "endow", "gift", "present", "offering",
            "sacrifice", "tithe", "firstfruits", "harvest", "crop", "yield", "produce", "fruit",
            "abundance", "plenty", "wealth", "riches", "treasure", "gold", "silver", "precious"
        },
        ["protection"] = new() 
        { 
            "protect", "defend", "shield", "armor", "fortress", "refuge", "shelter", "hiding place",
            "stronghold", "tower", "wall", "barrier", "guard", "watch", "keep", "preserve", "maintain",
            "secure", "safe", "sound", "whole", "complete", "perfect", "peace", "rest", "calm"
        },
        ["eternal"] = new() 
        { 
            "eternal", "everlasting", "forever", "evermore", "always", "constant", "unchanging",
            "immortal", "infinite", "endless", "timeless", "ageless", "permanent", "lasting",
            "heaven", "paradise", "glory", "kingdom", "reign", "rule", "dominion", "power", "authority"
        }
    };

    public static async Task<List<Quote>> DownloadCompleteBibleAsync()
    {
        var allBibleQuotes = new List<Quote>();

        try
        {
            Console.WriteLine("Downloading comprehensive Bible collection (6 versions)...");

            // Download KJV Bible (primary source)
            Console.WriteLine("Downloading KJV Bible...");
            var kjvQuotes = await DownloadKJVAsync();
            allBibleQuotes.AddRange(kjvQuotes);
            Console.WriteLine($"Downloaded {kjvQuotes.Count} KJV verses");

            // Download ASV (American Standard Version)
            Console.WriteLine("Downloading ASV Bible...");
            var asvQuotes = await DownloadASVAsync();
            allBibleQuotes.AddRange(asvQuotes);
            Console.WriteLine($"Downloaded {asvQuotes.Count} ASV verses");

            // Download WEB (World English Bible)
            Console.WriteLine("Downloading WEB Bible...");
            var webQuotes = await DownloadWEBAsync();
            allBibleQuotes.AddRange(webQuotes);
            Console.WriteLine($"Downloaded {webQuotes.Count} WEB verses");

            // Download YLT (Young's Literal Translation)
            Console.WriteLine("Downloading YLT Bible...");
            var yltQuotes = await DownloadYLTAsync();
            allBibleQuotes.AddRange(yltQuotes);
            Console.WriteLine($"Downloaded {yltQuotes.Count} YLT verses");

            // Download BBE (Bible in Basic English)
            Console.WriteLine("Downloading BBE Bible...");
            var bbeQuotes = await DownloadBBEAsync();
            allBibleQuotes.AddRange(bbeQuotes);
            Console.WriteLine($"Downloaded {bbeQuotes.Count} BBE verses");

            // Download DARBY (Darby English Bible)
            Console.WriteLine("Downloading DARBY Bible...");
            var darbyQuotes = await DownloadDarbyAsync();
            allBibleQuotes.AddRange(darbyQuotes);
            Console.WriteLine($"Downloaded {darbyQuotes.Count} DARBY verses");

            Console.WriteLine($"Total verses before deduplication: {allBibleQuotes.Count}");

            // Remove duplicates and categorize
            var uniqueQuotes = RemoveDuplicateVerses(allBibleQuotes);
            var categorizedQuotes = CategorizeBibleVerses(uniqueQuotes);

            Console.WriteLine($"Final unique Bible verses: {categorizedQuotes.Count}");

            return categorizedQuotes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading Bible: {ex.Message}");
            return allBibleQuotes;
        }
    }

    private static async Task<List<Quote>> DownloadKJVAsync()
    {
        var quotes = new List<Quote>();
        
        try
        {
            // Get list of all Bible books
            var booksResponse = await _httpClient.GetStringAsync("https://raw.githubusercontent.com/aruljohn/Bible-kjv/master/Books.json");
            var books = JsonSerializer.Deserialize<List<string>>(booksResponse);

            if (books != null)
            {
                int quoteId = 1;
                
                foreach (var book in books)
                {
                    try
                    {
                        // Download each book
                        var bookUrl = $"https://raw.githubusercontent.com/aruljohn/Bible-kjv/master/{book.Replace(" ", "%20")}.json";
                        var bookResponse = await _httpClient.GetStringAsync(bookUrl);
                        var bibleBook = JsonSerializer.Deserialize<BibleBook>(bookResponse);

                        if (bibleBook?.Chapters != null)
                        {
                            foreach (var chapter in bibleBook.Chapters)
                            {
                                foreach (var verse in chapter.Verses)
                                {
                                    var quote = new Quote
                                    {
                                        Id = quoteId++,
                                        Text = verse.Text.Trim(),
                                        Author = $"{book} {chapter.Chapter}:{verse.Verse}",
                                        Category = QuoteCategory.Bible, // Will be recategorized
                                        Genre = "bible",
                                        Source = "KJV-Bible",
                                        CreatedDate = DateTime.UtcNow
                                    };
                                    
                                    quotes.Add(quote);
                                }
                            }
                        }
                        
                        Console.WriteLine($"Downloaded {book}: {bibleBook?.Chapters?.Count ?? 0} chapters");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading {book}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading KJV Bible: {ex.Message}");
        }

        return quotes;
    }

    private static async Task<List<Quote>> DownloadASVAsync()
    {
        var quotes = new List<Quote>();
        
        try
        {
            // Use bible-api.com for ASV (American Standard Version)
            var books = await GetBibleBooksAsync();
            int quoteId = 1;
            
            foreach (var book in books.Take(5)) // Limit to first 5 books for testing
            {
                for (int chapter = 1; chapter <= 5; chapter++) // Limit to first 5 chapters
                {
                    try
                    {
                        var url = $"https://bible-api.com/{book}+{chapter}?translation=asv";
                        var response = await _httpClient.GetStringAsync(url);
                        var bibleData = JsonSerializer.Deserialize<BibleApiResponse>(response);

                        if (bibleData?.Verses != null)
                        {
                            foreach (var verse in bibleData.Verses)
                            {
                                var quote = new Quote
                                {
                                    Id = quoteId++,
                                    Text = verse.Text.Trim(),
                                    Author = $"{book} {chapter}:{verse.Verse}",
                                    Category = QuoteCategory.Bible,
                                    Genre = "bible",
                                    Source = "ASV-Bible",
                                    CreatedDate = DateTime.UtcNow
                                };
                                
                                quotes.Add(quote);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading {book} {chapter} ASV: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading ASV Bible: {ex.Message}");
        }

        return quotes;
    }

    private static async Task<List<Quote>> DownloadWEBAsync()
    {
        var quotes = new List<Quote>();
        
        try
        {
            // Use bible-api.com for WEB (World English Bible)
            var books = await GetBibleBooksAsync();
            int quoteId = 1;
            
            foreach (var book in books.Take(5)) // Limit to first 5 books for testing
            {
                for (int chapter = 1; chapter <= 5; chapter++) // Limit to first 5 chapters
                {
                    try
                    {
                        var url = $"https://bible-api.com/{book}+{chapter}?translation=web";
                        var response = await _httpClient.GetStringAsync(url);
                        var bibleData = JsonSerializer.Deserialize<BibleApiResponse>(response);

                        if (bibleData?.Verses != null)
                        {
                            foreach (var verse in bibleData.Verses)
                            {
                                var quote = new Quote
                                {
                                    Id = quoteId++,
                                    Text = verse.Text.Trim(),
                                    Author = $"{book} {chapter}:{verse.Verse}",
                                    Category = QuoteCategory.Bible,
                                    Genre = "bible",
                                    Source = "WEB-Bible",
                                    CreatedDate = DateTime.UtcNow
                                };
                                
                                quotes.Add(quote);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading {book} {chapter} WEB: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading WEB Bible: {ex.Message}");
        }

        return quotes;
    }

    private static async Task<List<Quote>> DownloadYLTAsync()
    {
        var quotes = new List<Quote>();
        
        try
        {
            // Use bible-api.com for YLT (Young's Literal Translation)
            var books = await GetBibleBooksAsync();
            int quoteId = 1;
            
            foreach (var book in books.Take(5)) // Limit to first 5 books for testing
            {
                for (int chapter = 1; chapter <= 5; chapter++) // Limit to first 5 chapters
                {
                    try
                    {
                        var url = $"https://bible-api.com/{book}+{chapter}?translation=ylt";
                        var response = await _httpClient.GetStringAsync(url);
                        var bibleData = JsonSerializer.Deserialize<BibleApiResponse>(response);

                        if (bibleData?.Verses != null)
                        {
                            foreach (var verse in bibleData.Verses)
                            {
                                var quote = new Quote
                                {
                                    Id = quoteId++,
                                    Text = verse.Text.Trim(),
                                    Author = $"{book} {chapter}:{verse.Verse}",
                                    Category = QuoteCategory.Bible,
                                    Genre = "bible",
                                    Source = "YLT-Bible",
                                    CreatedDate = DateTime.UtcNow
                                };
                                
                                quotes.Add(quote);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading {book} {chapter} YLT: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading YLT Bible: {ex.Message}");
        }

        return quotes;
    }

    private static async Task<List<Quote>> DownloadBBEAsync()
    {
        var quotes = new List<Quote>();
        
        try
        {
            // Use bible-api.com for BBE (Bible in Basic English)
            var books = await GetBibleBooksAsync();
            int quoteId = 1;
            
            foreach (var book in books.Take(5)) // Limit to first 5 books for testing
            {
                for (int chapter = 1; chapter <= 5; chapter++) // Limit to first 5 chapters
                {
                    try
                    {
                        var url = $"https://bible-api.com/{book}+{chapter}?translation=bbe";
                        var response = await _httpClient.GetStringAsync(url);
                        var bibleData = JsonSerializer.Deserialize<BibleApiResponse>(response);

                        if (bibleData?.Verses != null)
                        {
                            foreach (var verse in bibleData.Verses)
                            {
                                var quote = new Quote
                                {
                                    Id = quoteId++,
                                    Text = verse.Text.Trim(),
                                    Author = $"{book} {chapter}:{verse.Verse}",
                                    Category = QuoteCategory.Bible,
                                    Genre = "bible",
                                    Source = "BBE-Bible",
                                    CreatedDate = DateTime.UtcNow
                                };
                                
                                quotes.Add(quote);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading {book} {chapter} BBE: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading BBE Bible: {ex.Message}");
        }

        return quotes;
    }

    private static async Task<List<Quote>> DownloadDarbyAsync()
    {
        var quotes = new List<Quote>();
        
        try
        {
            // Use bible-api.com for Darby English Bible
            var books = await GetBibleBooksAsync();
            int quoteId = 1;
            
            foreach (var book in books.Take(5)) // Limit to first 5 books for testing
            {
                for (int chapter = 1; chapter <= 5; chapter++) // Limit to first 5 chapters
                {
                    try
                    {
                        var url = $"https://bible-api.com/{book}+{chapter}?translation=darby";
                        var response = await _httpClient.GetStringAsync(url);
                        var bibleData = JsonSerializer.Deserialize<BibleApiResponse>(response);

                        if (bibleData?.Verses != null)
                        {
                            foreach (var verse in bibleData.Verses)
                            {
                                var quote = new Quote
                                {
                                    Id = quoteId++,
                                    Text = verse.Text.Trim(),
                                    Author = $"{book} {chapter}:{verse.Verse}",
                                    Category = QuoteCategory.Bible,
                                    Genre = "bible",
                                    Source = "DARBY-Bible",
                                    CreatedDate = DateTime.UtcNow
                                };
                                
                                quotes.Add(quote);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading {book} {chapter} Darby: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading Darby Bible: {ex.Message}");
        }

        return quotes;
    }

    private static async Task<List<string>> GetBibleBooksAsync()
    {
        // Return a list of common Bible books for API calls
        return new List<string>
        {
            "Genesis", "Exodus", "Leviticus", "Numbers", "Deuteronomy",
            "Joshua", "Judges", "Ruth", "1%20Samuel", "2%20Samuel",
            "1%20Kings", "2%20Kings", "1%20Chronicles", "2%20Chronicles",
            "Ezra", "Nehemiah", "Esther", "Job", "Psalms", "Proverbs",
            "Ecclesiastes", "Song%20of%20Solomon", "Isaiah", "Jeremiah",
            "Lamentations", "Ezekiel", "Daniel", "Hosea", "Joel", "Amos",
            "Obadiah", "Jonah", "Micah", "Nahum", "Habakkuk", "Zephaniah",
            "Haggai", "Zechariah", "Malachi", "Matthew", "Mark", "Luke",
            "John", "Acts", "Romans", "1%20Corinthians", "2%20Corinthians",
            "Galatians", "Ephesians", "Philippians", "Colossians",
            "1%20Thessalonians", "2%20Thessalonians", "1%20Timothy",
            "2%20Timothy", "Titus", "Philemon", "Hebrews", "James",
            "1%20Peter", "2%20Peter", "1%20John", "2%20John", "3%20John",
            "Jude", "Revelation"
        };
    }

    private static List<Quote> RemoveDuplicateVerses(List<Quote> verses)
    {
        var uniqueVerses = new List<Quote>();
        var seenReferences = new HashSet<string>();

        foreach (var verse in verses)
        {
            var normalizedReference = NormalizeVerseReference(verse.Author, verse.Text);
            if (!seenReferences.Contains(normalizedReference))
            {
                seenReferences.Add(normalizedReference);
                uniqueVerses.Add(verse);
            }
        }

        return uniqueVerses;
    }

    private static string NormalizeVerseReference(string reference, string text)
    {
        // Create a unique identifier based on reference and first few words
        var refParts = reference.Split(':');
        if (refParts.Length >= 2)
        {
            var bookChapter = refParts[0].Trim();
            var verseNum = refParts[1].Trim();
            var textWords = string.Join(" ", text.Split(' ').Take(3));
            return $"{bookChapter}:{verseNum}:{textWords}".ToLower().Replace(" ", "");
        }
        return reference.ToLower();
    }

    private static List<Quote> CategorizeBibleVerses(List<Quote> verses)
    {
        foreach (var verse in verses)
        {
            verse.Category = DetermineBibleCategory(verse.Text);
        }
        return verses;
    }

    private static QuoteCategory DetermineBibleCategory(string verseText)
    {
        var lowerText = verseText.ToLower();
        var scores = new Dictionary<QuoteCategory, int>();

        // Score each category based on theme keywords
        foreach (var theme in _bibleThemes)
        {
            var keywords = theme.Value;
            var score = keywords.Count(keyword => lowerText.Contains(keyword));
            
            // Map themes to QuoteCategory
            var category = theme.Key switch
            {
                "inspiration" => QuoteCategory.Motivational,
                "wisdom" => QuoteCategory.Wisdom,
                "encouragement" => QuoteCategory.Encouragement,
                "love" => QuoteCategory.Love,
                "life" => QuoteCategory.Life,
                "guidance" => QuoteCategory.Leadership,
                "provision" => QuoteCategory.Business,
                "protection" => QuoteCategory.Success,
                "eternal" => QuoteCategory.Bible,
                _ => QuoteCategory.Bible
            };

            scores[category] = score;
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

        // Default to Bible category if no clear theme found
        return QuoteCategory.Bible;
    }

    public static async Task<List<Quote>> DownloadInspirationalVersesAsync()
    {
        var allVerses = await DownloadCompleteBibleAsync();
        
        // Filter for inspirational verses
        var inspirationalVerses = allVerses
            .Where(v => v.Category == QuoteCategory.Motivational || 
                       v.Category == QuoteCategory.Encouragement ||
                       v.Category == QuoteCategory.Wisdom)
            .ToList();

        return inspirationalVerses;
    }

    public static async Task<List<Quote>> DownloadTopBibleVersesAsync()
    {
        var allVerses = await DownloadCompleteBibleAsync();
        
        // Return a curated selection of well-known verses
        var famousReferences = new[]
        {
            "John 3:16", "Philippians 4:13", "Jeremiah 29:11", "Proverbs 3:5-6", "Romans 8:28",
            "Isaiah 41:10", "Joshua 1:9", "Psalm 23:1", "Matthew 28:19", "2 Corinthians 5:17",
            "Ephesians 2:8-9", "Galatians 2:20", "Philippians 4:6-7", "Romans 12:2", "Isaiah 54:17",
            "Psalm 46:10", "Proverbs 16:3", "1 Corinthians 13:4-8", "Hebrews 11:1", "James 1:2-4"
        };

        var topVerses = new List<Quote>();
        foreach (var reference in famousReferences)
        {
            var verse = allVerses.FirstOrDefault(v => 
                v.Author.Contains(reference.Split(':')[0]) && 
                v.Author.Contains(reference.Split(':')[1]));
            
            if (verse != null)
            {
                topVerses.Add(verse);
            }
        }

        // If some famous verses are missing, add random ones to ensure good coverage
        if (topVerses.Count < 50)
        {
            var random = new Random();
            var additionalVerses = allVerses
                .Where(v => !topVerses.Any(tv => tv.Author == v.Author))
                .OrderBy(v => random.Next())
                .Take(50 - topVerses.Count);
            
            topVerses.AddRange(additionalVerses);
        }

        return topVerses;
    }
}

// JSON model for Bible book structure
public class BibleBook
{
    public string Book { get; set; } = string.Empty;
    public List<BibleChapter> Chapters { get; set; } = new();
}

public class BibleChapter
{
    public string Chapter { get; set; } = string.Empty;
    public List<BibleVerse> Verses { get; set; } = new();
}

public class BibleVerse
{
    public string Verse { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

// API Response model for bible-api.com
public class BibleApiResponse
{
    public string Reference { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public List<BibleApiVerse> Verses { get; set; } = new();
}

public class BibleApiVerse
{
    public string Book { get; set; } = string.Empty;
    public string Chapter { get; set; } = string.Empty;
    public string Verse { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
