using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelPulse.Database;
using PixelPulse.Models;

namespace PixelPulse.DataIngestion;

public class BibleImporter
{
    private readonly QuoteDbContext _context;
    private readonly HttpClient _httpClient;

    public BibleImporter(QuoteDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
    }

    public async Task<int> ImportBibleVersesAsync(Action<string>? progressCallback = null)
    {
        progressCallback?.Invoke("Downloading Bible data from GitHub...");
        
        try
        {
            // Download BibleInJson data
            var chaptersUrl = "https://raw.githubusercontent.com/swvincent/BibleInJson/main/chapters.json";
            var response = await _httpClient.GetAsync(chaptersUrl);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            progressCallback?.Invoke("Parsing Bible data...");
            
            // Bible JSON is an array of chapter objects
            var chaptersArray = JsonConvert.DeserializeObject<JArray>(json);
            if (chaptersArray == null)
            {
                progressCallback?.Invoke("Failed to parse Bible data.");
                return 0;
            }

            int imported = 0;
            int totalVerses = 0;

            // Sample first chapter to understand structure
            if (chaptersArray.Count > 0)
            {
                var sample = chaptersArray[0];
                var sampleKeys = string.Join(", ", ((JObject)sample).Properties().Select(p => p.Name));
                progressCallback?.Invoke($"Sample chapter keys: {sampleKeys}");
            }

            foreach (var chapterObj in chaptersArray)
            {
                var bookName = chapterObj["bookName"]?.Value<string>() ?? 
                              chapterObj["book"]?.Value<string>() ??
                              "Unknown";
                
                // Try different field names for chapter number
                var chapterNumber = chapterObj["chapter"]?.Value<int>() ?? 
                                   chapterObj["chapterNumber"]?.Value<int>() ??
                                   0;
                
                // Try different possible field names for verses
                var verses = chapterObj["verses"] as JArray ?? 
                            chapterObj["Verses"] as JArray ??
                            chapterObj["verse"] as JArray;
                
                if (verses == null)
                {
                    // Check if verses might be in a different structure
                    var versesObj = chapterObj["verses"] as JObject;
                    if (versesObj != null)
                    {
                        // Verses might be an object with verse numbers as keys
                        verses = new JArray();
                        foreach (var prop in versesObj.Properties())
                        {
                            if (int.TryParse(prop.Name, out _))
                            {
                                verses.Add(prop.Value);
                            }
                        }
                    }
                }
                
                if (verses == null || verses.Count == 0)
                {
                    continue;
                }

                foreach (var verseObj in verses)
                {
                    totalVerses++;
                    
                    // Handle both object and string verse formats
                    string verseText;
                    int verseNumber = 0;
                    
                    if (verseObj.Type == JTokenType.String)
                    {
                        // Verse is just a string, use index as verse number
                        verseText = verseObj.Value<string>() ?? string.Empty;
                        verseNumber = totalVerses % 1000; // Approximate verse number
                    }
                    else if (verseObj.Type == JTokenType.Object)
                    {
                        // Try different field names for verse number and text
                        verseNumber = verseObj["verse"]?.Value<int>() ?? 
                                     verseObj["verseNumber"]?.Value<int>() ??
                                     verseObj["Verse"]?.Value<int>() ??
                                     verseObj["number"]?.Value<int>() ?? 0;
                        
                        verseText = verseObj["text"]?.Value<string>() ?? 
                                   verseObj["Text"]?.Value<string>() ??
                                   verseObj["verseText"]?.Value<string>() ??
                                   verseObj["content"]?.Value<string>() ??
                                   verseObj.ToString();
                    }
                    else
                    {
                        verseText = verseObj.ToString();
                    }
                    
                    if (string.IsNullOrWhiteSpace(verseText) || verseText.Length < 3)
                        continue;

                    // Check if verse already exists
                    var reference = $"{bookName} {chapterNumber}:{verseNumber}";
                    var exists = await _context.Quotes
                        .AnyAsync(q => q.Text == verseText && 
                                     q.Author == reference && 
                                     q.Category == QuoteCategory.Bible);

                    if (!exists)
                    {
                        var quote = new Quote
                        {
                            Text = verseText,
                            Author = reference,
                            Category = QuoteCategory.Bible,
                            Genre = GetBibleTopic(verseText),
                            Source = "BibleInJson",
                            CreatedDate = DateTime.UtcNow
                        };

                        _context.Quotes.Add(quote);
                        imported++;

                        if (imported % 500 == 0)
                        {
                            await _context.SaveChangesAsync();
                            progressCallback?.Invoke($"Imported {imported} Bible verses...");
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            progressCallback?.Invoke($"Bible import complete. Imported {imported} verses from {totalVerses} total.");
            return imported;
        }
        catch (Exception ex)
        {
            progressCallback?.Invoke($"Error importing Bible: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Bible import error: {ex}");
            return 0;
        }
    }

    private string GetBookName(int bookNumber)
    {
        var books = new[]
        {
            "Genesis", "Exodus", "Leviticus", "Numbers", "Deuteronomy", "Joshua", "Judges", "Ruth",
            "1 Samuel", "2 Samuel", "1 Kings", "2 Kings", "1 Chronicles", "2 Chronicles", "Ezra", "Nehemiah",
            "Esther", "Job", "Psalms", "Proverbs", "Ecclesiastes", "Song of Solomon", "Isaiah", "Jeremiah",
            "Lamentations", "Ezekiel", "Daniel", "Hosea", "Joel", "Amos", "Obadiah", "Jonah", "Micah", "Nahum",
            "Habakkuk", "Zephaniah", "Haggai", "Zechariah", "Malachi", "Matthew", "Mark", "Luke", "John",
            "Acts", "Romans", "1 Corinthians", "2 Corinthians", "Galatians", "Ephesians", "Philippians",
            "Colossians", "1 Thessalonians", "2 Thessalonians", "1 Timothy", "2 Timothy", "Titus", "Philemon",
            "Hebrews", "James", "1 Peter", "2 Peter", "1 John", "2 John", "3 John", "Jude", "Revelation"
        };

        if (bookNumber > 0 && bookNumber <= books.Length)
            return books[bookNumber - 1];
        
        return $"Book {bookNumber}";
    }

    private string GetBibleTopic(string verseText)
    {
        var textLower = verseText.ToLower();
        
        if (textLower.Contains("love") || textLower.Contains("beloved"))
            return "love";
        if (textLower.Contains("faith") || textLower.Contains("believe"))
            return "faith";
        if (textLower.Contains("hope") || textLower.Contains("trust"))
            return "hope";
        if (textLower.Contains("wisdom") || textLower.Contains("wise"))
            return "wisdom";
        if (textLower.Contains("encourage") || textLower.Contains("strength") || textLower.Contains("courage"))
            return "encouragement";
        if (textLower.Contains("peace") || textLower.Contains("rest"))
            return "peace";
        if (textLower.Contains("joy") || textLower.Contains("rejoice"))
            return "joy";
        if (textLower.Contains("pray") || textLower.Contains("prayer"))
            return "prayer";
        
        return "general";
    }
}
