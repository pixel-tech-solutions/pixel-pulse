using System;
using System.Windows;
using System.Windows.Forms;
using PixelPulse.Models;
using Application = System.Windows.Application;

namespace PixelPulse
{
    public partial class App : Application
    {
        private NotifyIcon? _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Check Windows version
            if (!IsWindows10OrLater())
            {
                System.Windows.MessageBox.Show("Pixel Pulse requires Windows 10 or later to run.", "Pixel Pulse", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // Show splash screen
            var splash = new SplashScreen();
            splash.Show();
            
            // Use dispatcher timer to close splash after delay
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                splash.Close();
            
                // Create and show main window
                var mainWindow = new MainWindow();
                mainWindow.Show();
                MainWindow = mainWindow;
            };
            timer.Start();

            // Create system tray icon
            _notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("Resources\\icon.ico"),
                Visible = true,
                Text = CompanyInfo.ProductName
            };

            _notifyIcon.DoubleClick += (s, args) =>
            {
                MainWindow?.Show();
                MainWindow?.Activate();
            };

            // Create context menu for tray icon
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Settings", null, (s, args) =>
            {
                if (MainWindow is MainWindow mw)
                {
                    var settingsManager = new SettingsManager();
                    var currentSettings = settingsManager.LoadSettings();
                    var settingsWindow = new SettingsWindow(settingsManager, currentSettings);
                    settingsWindow.ShowDialog();
                }
            });
            contextMenu.Items.Add("Refresh Quote", null, (s, args) =>
            {
                if (MainWindow is MainWindow mw)
                {
                    mw.Show();
                    mw.Activate();
                    mw.LoadQuote();
                }
            });
            contextMenu.Items.Add("About", null, (s, args) =>
            {
                var aboutWindow = new AboutWindow { Owner = MainWindow };
                aboutWindow.ShowDialog();
            });
            contextMenu.Items.Add("Exit", null, (s, args) => Shutdown());
            _notifyIcon.ContextMenuStrip = contextMenu;

            // Ensure single instance
            EnsureSingleInstance();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
            base.OnExit(e);
        }

        private static System.Threading.Mutex? _singleInstanceMutex;

        private void EnsureSingleInstance()
        {
            _singleInstanceMutex = new System.Threading.Mutex(true, "PixelPulse_SingleInstance", out bool isNewInstance);
            if (!isNewInstance)
            {
                System.Windows.MessageBox.Show("Pixel Pulse is already running.", "Pixel Pulse", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
            }
        }

        private bool IsWindows10OrLater()
        {
            try
            {
                var version = Environment.OSVersion.Version;
                return version.Major >= 10;
            }
            catch
            {
                // If version check fails, assume Windows 10+ for safety
                return true;
            }
        }
    }
}
