using System;
using System.IO;
using Newtonsoft.Json;
using PixelPulse.Models;

namespace PixelPulse;

public class SettingsManager
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PixelPulse",
        "settings.json"
    );

    private Settings? _currentSettings;

    public Settings LoadSettings()
    {
        if (_currentSettings != null)
            return _currentSettings;

        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                _currentSettings = JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
            }
            else
            {
                _currentSettings = new Settings();
                SaveSettings(_currentSettings);
            }
        }
        catch
        {
            _currentSettings = new Settings();
        }

        return _currentSettings;
    }

    public void SaveSettings(Settings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
            _currentSettings = settings;
        }
        catch (Exception ex)
        {
            // Log error (implement logging later)
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    public event EventHandler<Settings>? SettingsChanged;

    public void NotifySettingsChanged(Settings settings)
    {
        SettingsChanged?.Invoke(this, settings);
    }
}
