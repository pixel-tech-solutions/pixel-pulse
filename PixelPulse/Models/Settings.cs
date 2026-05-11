using System.Collections.Generic;

namespace PixelPulse.Models;

public class Settings
{
    public int FontSize { get; set; } = 18;
    public string FontFamily { get; set; } = "Segoe UI";
    public string TextColor { get; set; } = "#FFFFFF";
    public int RefreshIntervalSeconds { get; set; } = 60;
    public string Position { get; set; } = "TopRight";
    public bool StartWithWindows { get; set; } = true;
    public bool AlwaysOnTop { get; set; } = false;
    public bool ClickThrough { get; set; } = true;
    public int FadeAfterSeconds { get; set; } = 30;
    public bool HoverBrighten { get; set; } = true;
    public List<QuoteCategory> EnabledCategories { get; set; } = new List<QuoteCategory>
    {
        QuoteCategory.Motivational,
        QuoteCategory.Bible,
        QuoteCategory.Love,
        QuoteCategory.Tech,
        QuoteCategory.Business,
        QuoteCategory.Success,
        QuoteCategory.Leadership,
        QuoteCategory.Wisdom,
        QuoteCategory.Encouragement,
        QuoteCategory.Life
    };
    public QuoteMatchMode MatchMode { get; set; } = QuoteMatchMode.None;
    public QuoteDisplayFormat DisplayFormat { get; set; } = QuoteDisplayFormat.Stacked;
    public QuoteCategory? PrimaryMatchCategory { get; set; }
    public QuoteCategory? SecondaryMatchCategory { get; set; }
}
