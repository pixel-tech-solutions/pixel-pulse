using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using PixelPulse.Database;
using PixelPulse.Models;

namespace PixelPulse;

public partial class MainWindow : Window
{
    private readonly QuoteService _quoteService;
    private readonly SettingsManager _settingsManager;
    private Settings _settings;
    private DispatcherTimer? _refreshTimer;
    private DispatcherTimer? _fadeTimer;
    private DispatcherTimer? _alternatingTimer;
    private bool _isHovering = false;
    private bool _showPrimaryQuote = true;
    private Quote? _currentQuote;
    private (Quote Primary, Quote Secondary)? _currentMatchedQuotes;

    public MainWindow()
    {
        InitializeComponent();
        
        _settingsManager = new SettingsManager();
        _settings = _settingsManager.LoadSettings();
        
        // Ensure database exists
        var context = new QuoteDbContext();
        context.Database.EnsureCreated();
        _quoteService = new QuoteService(context);
        
        _settingsManager.SettingsChanged += (s, newSettings) =>
        {
            _settings = newSettings;
            ApplySettings();
        };
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ApplySettings();
        LoadQuote();
        StartTimers();
    }

    private void ApplySettings()
    {
        try
        {
            // Position
            PositionWindow(_settings.Position);

            // Window properties
            Topmost = _settings.AlwaysOnTop;
            
            // Click-through
            UpdateClickThrough();

            // Font
            QuoteTextBlock.FontSize = _settings.FontSize;
            QuoteTextBlock.FontFamily = new System.Windows.Media.FontFamily(_settings.FontFamily);
            
            // Color
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_settings.TextColor);
                QuoteTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(color);
            }
            catch
            {
                // Default to white if color parsing fails
                QuoteTextBlock.Foreground = System.Windows.Media.Brushes.White;
            }

            // Update quote display
            UpdateQuoteDisplay();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying settings: {ex.Message}");
        }
    }

    private void PositionWindow(string position)
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        switch (position)
        {
            case "TopRight":
                Left = screenWidth - Width - 20;
                Top = 20;
                break;
            case "TopLeft":
                Left = 20;
                Top = 20;
                break;
            case "BottomRight":
                Left = screenWidth - Width - 20;
                Top = screenHeight - Height - 50;
                break;
            case "BottomLeft":
                Left = 20;
                Top = screenHeight - Height - 50;
                break;
        }
    }

    public void LoadQuote()
    {
        try
        {
            _showPrimaryQuote = true; // Reset alternating display
            
            if (_settings.MatchMode == QuoteMatchMode.None)
            {
                _currentQuote = _quoteService.GetRandomQuote(_settings.EnabledCategories);
                _currentMatchedQuotes = null;
            }
            else if (_settings.MatchMode == QuoteMatchMode.Custom && 
                     _settings.PrimaryMatchCategory.HasValue && 
                     _settings.SecondaryMatchCategory.HasValue)
            {
                _currentMatchedQuotes = _quoteService.GetMatchedQuotes(
                    _settings.PrimaryMatchCategory.Value,
                    _settings.SecondaryMatchCategory.Value);
                _currentQuote = null;
            }
            else
            {
                _currentMatchedQuotes = _quoteService.GetMatchedQuotes(_settings.MatchMode);
                _currentQuote = null;
            }

            UpdateQuoteDisplay();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading quote: {ex.Message}");
            QuoteTextBlock.Text = "Error loading quote. Please check database.";
        }
    }

    private void UpdateQuoteDisplay()
    {
        if (_currentMatchedQuotes.HasValue)
        {
            var (primary, secondary) = _currentMatchedQuotes.Value;
            switch (_settings.DisplayFormat)
            {
                case QuoteDisplayFormat.Stacked:
                    QuoteTextBlock.Text = $"{primary.Text}\n\n— {primary.Author ?? "Unknown"}\n\n{secondary.Text}\n\n— {secondary.Author ?? "Unknown"}";
                    break;
                case QuoteDisplayFormat.SideBySide:
                    QuoteTextBlock.Text = $"{primary.Text} — {primary.Author ?? "Unknown"} | {secondary.Text} — {secondary.Author ?? "Unknown"}";
                    break;
                case QuoteDisplayFormat.Alternating:
                    var quoteToShow = _showPrimaryQuote ? primary : secondary;
                    QuoteTextBlock.Text = $"{quoteToShow.Text}\n\n— {quoteToShow.Author ?? "Unknown"}";
                    StartAlternatingTimer();
                    break;
            }
        }
        else if (_currentQuote != null)
        {
            QuoteTextBlock.Text = $"{_currentQuote.Text}\n\n— {_currentQuote.Author ?? "Unknown"}";
            StopAlternatingTimer();
        }
        else
        {
            QuoteTextBlock.Text = "No quotes available. Please run the database builder.";
            StopAlternatingTimer();
        }
    }

    private void StartAlternatingTimer()
    {
        if (_settings.DisplayFormat != QuoteDisplayFormat.Alternating || _currentMatchedQuotes == null)
        {
            StopAlternatingTimer();
            return;
        }

        _alternatingTimer?.Stop();
        _alternatingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10) // Switch every 10 seconds
        };
        _alternatingTimer.Tick += (s, e) =>
        {
            if (_currentMatchedQuotes.HasValue)
            {
                _showPrimaryQuote = !_showPrimaryQuote;
                var quoteToShow = _showPrimaryQuote ? _currentMatchedQuotes.Value.Primary : _currentMatchedQuotes.Value.Secondary;
                QuoteTextBlock.Text = $"{quoteToShow.Text}\n\n— {quoteToShow.Author ?? "Unknown"}";
            }
        };
        _alternatingTimer.Start();
    }

    private void StopAlternatingTimer()
    {
        _alternatingTimer?.Stop();
        _alternatingTimer = null;
    }

    private void StartTimers()
    {
        // Refresh timer
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(_settings.RefreshIntervalSeconds)
        };
        _refreshTimer.Tick += (s, e) => LoadQuote();
        _refreshTimer.Start();

        // Fade timer
        if (_settings.FadeAfterSeconds > 0)
        {
            _fadeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_settings.FadeAfterSeconds)
            };
            _fadeTimer.Tick += (s, e) =>
            {
                if (!_isHovering)
                {
                    var fadeOut = (Storyboard)FindResource("FadeOutAnimation");
                    fadeOut?.Begin();
                }
            };
            _fadeTimer.Start();
        }
    }

    private void Window_MouseEnter(object sender, MouseEventArgs e)
    {
        _isHovering = true;
        if (_settings.HoverBrighten)
        {
            var fadeIn = (Storyboard)FindResource("FadeInAnimation");
            fadeIn?.Begin();
        }
    }

    private void Window_MouseLeave(object sender, MouseEventArgs e)
    {
        _isHovering = false;
    }

    private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // Temporarily disable click-through to show context menu
            if (_settings.ClickThrough)
            {
                var helper = new WindowInteropHelper(this);
                if (helper.Handle != IntPtr.Zero)
                {
                    const int GWL_EXSTYLE = -20;
                    const int WS_EX_TRANSPARENT = 0x00000020;
                    var exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
                    SetWindowLong(helper.Handle, GWL_EXSTYLE, (IntPtr)(exStyle & ~WS_EX_TRANSPARENT));
                }
            }
            
            ContextMenu.IsOpen = true;
            e.Handled = true;
            
            // Re-enable click-through after menu closes
            ContextMenu.Closed += (s, args) =>
            {
                if (_settings.ClickThrough)
                {
                    UpdateClickThrough();
                }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing context menu: {ex.Message}");
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        UpdateClickThrough();
    }

    private void UpdateClickThrough()
    {
        if (_settings.ClickThrough)
        {
            try
            {
                var helper = new WindowInteropHelper(this);
                if (helper.Handle != IntPtr.Zero)
                {
                    const int GWL_EXSTYLE = -20;
                    const int WS_EX_TRANSPARENT = 0x00000020;
                    const int WS_EX_LAYERED = 0x00080000;
                    
                    var exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
                    SetWindowLong(helper.Handle, GWL_EXSTYLE, (IntPtr)(exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting click-through: {ex.Message}");
            }
        }
        else
        {
            try
            {
                var helper = new WindowInteropHelper(this);
                if (helper.Handle != IntPtr.Zero)
                {
                    const int GWL_EXSTYLE = -20;
                    const int WS_EX_TRANSPARENT = 0x00000020;
                    const int WS_EX_LAYERED = 0x00080000;
                    
                    var exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
                    SetWindowLong(helper.Handle, GWL_EXSTYLE, (IntPtr)(exStyle & ~WS_EX_TRANSPARENT));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing click-through: {ex.Message}");
            }
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_settingsManager, _settings);
        settingsWindow.ShowDialog();
        _settings = _settingsManager.LoadSettings();
        ApplySettings();
    }

    private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
    {
        LoadQuote();
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow { Owner = this };
        aboutWindow.ShowDialog();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
