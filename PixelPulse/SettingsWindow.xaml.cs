using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using PixelPulse.Models;

namespace PixelPulse;

public partial class SettingsWindow : Window
{
    private readonly SettingsManager _settingsManager;
    private Settings _settings;

    public SettingsWindow(SettingsManager settingsManager, Settings currentSettings)
    {
        InitializeComponent();
        _settingsManager = settingsManager;
        _settings = currentSettings;
        
        LoadSettings();
        InitializeControls();
    }

    private void InitializeControls()
    {
        // Initialize font families
        FontFamilyComboBox.ItemsSource = System.Windows.Media.Fonts.SystemFontFamilies.OrderBy(f => f.Source);
        FontFamilyComboBox.SelectedItem = System.Windows.Media.Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == _settings.FontFamily);

        // Initialize category combo boxes
        var categories = Enum.GetValues(typeof(QuoteCategory)).Cast<QuoteCategory>();
        PrimaryCategoryComboBox.ItemsSource = categories;
        SecondaryCategoryComboBox.ItemsSource = categories;
        
        if (_settings.PrimaryMatchCategory.HasValue)
            PrimaryCategoryComboBox.SelectedItem = _settings.PrimaryMatchCategory.Value;
        if (_settings.SecondaryMatchCategory.HasValue)
            SecondaryCategoryComboBox.SelectedItem = _settings.SecondaryMatchCategory.Value;
    }

    private void LoadSettings()
    {
        // Font settings
        FontSizeSlider.Value = _settings.FontSize;
        FontSizeText.Text = _settings.FontSize.ToString();
        ColorTextBox.Text = _settings.TextColor;

        // Display settings
        PositionComboBox.SelectedIndex = _settings.Position switch
        {
            "TopRight" => 0,
            "TopLeft" => 1,
            "BottomRight" => 2,
            "BottomLeft" => 3,
            _ => 0
        };
        AlwaysOnTopCheckBox.IsChecked = _settings.AlwaysOnTop;
        ClickThroughCheckBox.IsChecked = _settings.ClickThrough;
        HoverBrightenCheckBox.IsChecked = _settings.HoverBrighten;

        // Quote settings
        RefreshIntervalComboBox.SelectedIndex = _settings.RefreshIntervalSeconds switch
        {
            30 => 0,
            60 => 1,
            300 => 2,
            600 => 3,
            1800 => 4,
            3600 => 5,
            86400 => 6,
            _ => 1
        };
        
        FadeAfterComboBox.SelectedIndex = _settings.FadeAfterSeconds switch
        {
            30 => 0,
            60 => 1,
            300 => 2,
            600 => 3,
            1800 => 4,
            3600 => 5,
            86400 => 6,
            0 => 7,
            _ => 0
        };

        // Categories
        foreach (System.Windows.Controls.ListBoxItem item in CategoriesListBox.Items)
        {
            if (Enum.TryParse<QuoteCategory>(item.Tag?.ToString(), out var category))
            {
                item.IsSelected = _settings.EnabledCategories.Contains(category);
            }
        }

        // Match settings
        MatchModeComboBox.SelectedIndex = (int)_settings.MatchMode;
        DisplayFormatComboBox.SelectedIndex = (int)_settings.DisplayFormat;
        
        if (_settings.MatchMode == QuoteMatchMode.Custom)
        {
            CustomMatchPanel.Visibility = Visibility.Visible;
        }

        // Startup
        StartWithWindowsCheckBox.IsChecked = _settings.StartWithWindows;
    }

    private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        FontSizeText.Text = ((int)e.NewValue).ToString();
    }

    private void FontFamilyComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void ColorTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // Validate color format
    }

    private void PickColorButton_Click(object sender, RoutedEventArgs e)
    {
        var colorDialog = new ColorDialog();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var color = colorDialog.Color;
            ColorTextBox.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

    private void PositionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void AlwaysOnTopCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void AlwaysOnTopCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void ClickThroughCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void ClickThroughCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void HoverBrightenCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void HoverBrightenCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void RefreshIntervalComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void FadeAfterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void MatchModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (MatchModeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item)
        {
            var tag = item.Tag?.ToString();
            if (tag == "Custom")
            {
                CustomMatchPanel.Visibility = Visibility.Visible;
            }
            else
            {
                CustomMatchPanel.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void PrimaryCategoryComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void SecondaryCategoryComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void DisplayFormatComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void StartWithWindowsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void StartWithWindowsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        // Handled in SaveButton_Click
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Font settings
        _settings.FontSize = (int)FontSizeSlider.Value;
        if (FontFamilyComboBox.SelectedItem is FontFamily fontFamily)
        {
            _settings.FontFamily = fontFamily.Source;
        }
        _settings.TextColor = ColorTextBox.Text;

        // Display settings
        _settings.Position = PositionComboBox.SelectedIndex switch
        {
            0 => "TopRight",
            1 => "TopLeft",
            2 => "BottomRight",
            3 => "BottomLeft",
            _ => "TopRight"
        };
        _settings.AlwaysOnTop = AlwaysOnTopCheckBox.IsChecked ?? false;
        _settings.ClickThrough = ClickThroughCheckBox.IsChecked ?? true;
        _settings.HoverBrighten = HoverBrightenCheckBox.IsChecked ?? true;

        // Quote settings
        _settings.RefreshIntervalSeconds = RefreshIntervalComboBox.SelectedIndex switch
        {
            0 => 30,
            1 => 60,
            2 => 300,
            3 => 600,
            4 => 1800,
            5 => 3600,
            6 => 86400,
            _ => 60
        };

        _settings.FadeAfterSeconds = FadeAfterComboBox.SelectedIndex switch
        {
            0 => 30,
            1 => 60,
            2 => 300,
            3 => 600,
            4 => 1800,
            5 => 3600,
            6 => 86400,
            7 => 0,
            _ => 30
        };

        // Categories
        _settings.EnabledCategories = CategoriesListBox.SelectedItems
            .Cast<System.Windows.Controls.ListBoxItem>()
            .Where(item => item.Tag != null && Enum.TryParse<QuoteCategory>(item.Tag.ToString(), out _))
            .Select(item => Enum.Parse<QuoteCategory>(item.Tag!.ToString()!))
            .ToList();

        // Match settings
        if (MatchModeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem matchItem)
        {
            var matchTag = matchItem.Tag?.ToString();
            _settings.MatchMode = matchTag switch
            {
                "None" => QuoteMatchMode.None,
                "MotivationalBible" => QuoteMatchMode.MotivationalBible,
                "LoveMotivational" => QuoteMatchMode.LoveMotivational,
                "SuccessLeadership" => QuoteMatchMode.SuccessLeadership,
                "BusinessSuccess" => QuoteMatchMode.BusinessSuccess,
                "WisdomBible" => QuoteMatchMode.WisdomBible,
                "EncouragementBible" => QuoteMatchMode.EncouragementBible,
                "Custom" => QuoteMatchMode.Custom,
                _ => QuoteMatchMode.None
            };
        }

        if (_settings.MatchMode == QuoteMatchMode.Custom)
        {
            if (PrimaryCategoryComboBox.SelectedItem is QuoteCategory primary)
                _settings.PrimaryMatchCategory = primary;
            if (SecondaryCategoryComboBox.SelectedItem is QuoteCategory secondary)
                _settings.SecondaryMatchCategory = secondary;
        }

        if (DisplayFormatComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem formatItem)
        {
            var formatTag = formatItem.Tag?.ToString();
            _settings.DisplayFormat = formatTag switch
            {
                "Stacked" => QuoteDisplayFormat.Stacked,
                "SideBySide" => QuoteDisplayFormat.SideBySide,
                "Alternating" => QuoteDisplayFormat.Alternating,
                _ => QuoteDisplayFormat.Stacked
            };
        }

        // Startup
        _settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;
        
        if (_settings.StartWithWindows)
        {
            StartupManager.EnableStartup();
        }
        else
        {
            StartupManager.DisableStartup();
        }

        _settingsManager.SaveSettings(_settings);
        _settingsManager.NotifySettingsChanged(_settings);
        
        DialogResult = true;
        Close();
    }

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow { Owner = this };
        aboutWindow.ShowDialog();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
