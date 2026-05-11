using System.Windows.Media;

namespace PixelPulse;

public static class BrandColors
{
    // Primary brand colors
    public static readonly Color PrimaryPurple = Color.FromRgb(107, 70, 193);      // #6B46C1
    public static readonly Color SecondaryBlue = Color.FromRgb(59, 130, 246);      // #3B82F6
    public static readonly Color AccentGold = Color.FromRgb(245, 158, 11);         // #F59E0B
    
    // Background colors
    public static readonly Color DarkBackground = Color.FromRgb(31, 41, 55);        // #1F2937
    public static readonly Color LightBackground = Color.FromRgb(249, 250, 251);   // #F9FAFB
    
    // Text colors
    public static readonly Color LightText = Color.FromRgb(249, 250, 251);         // #F9FAFB
    public static readonly Color DarkText = Color.FromRgb(17, 24, 39);            // #111827
    
    // Brushes for easy use
    public static readonly SolidColorBrush PrimaryPurpleBrush = new SolidColorBrush(PrimaryPurple);
    public static readonly SolidColorBrush SecondaryBlueBrush = new SolidColorBrush(SecondaryBlue);
    public static readonly SolidColorBrush AccentGoldBrush = new SolidColorBrush(AccentGold);
    public static readonly SolidColorBrush DarkBackgroundBrush = new SolidColorBrush(DarkBackground);
    public static readonly SolidColorBrush LightTextBrush = new SolidColorBrush(LightText);
}
