using System.IO;
using System.Text.Json;
using LapKeys.Models;

namespace LapKeys.Services;

/// <summary>
/// Service for persisting application settings to disk.
/// </summary>
public static class SettingsService
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LapKeys");
    
    private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Loads settings from disk, or returns default settings if file doesn't exist.
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                return settings ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }
        
        return new AppSettings();
    }

    /// <summary>
    /// Saves settings to disk.
    /// </summary>
    public static void Save(AppSettings settings)
    {
        try
        {
            // Ensure directory exists
            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
            }
            
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
}
