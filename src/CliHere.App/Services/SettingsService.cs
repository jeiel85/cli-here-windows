using System.Text.Json;
using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsDirectoryPath;
    private readonly bool _isPortable;

    public SettingsService(string? settingsDirectoryPath = null)
    {
        if (!string.IsNullOrWhiteSpace(settingsDirectoryPath))
        {
            _settingsDirectoryPath = settingsDirectoryPath;
            _isPortable = true;
        }
        else if (IsPortableMode())
        {
            _settingsDirectoryPath = AppContext.BaseDirectory;
            _isPortable = true;
        }
        else
        {
            _settingsDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CliHere");
            _isPortable = false;
        }
    }

    public string SettingsDirectoryPath => _settingsDirectoryPath;
    public string SettingsFilePath => Path.Combine(SettingsDirectoryPath, "settings.json");
    public bool IsPortable => _isPortable;

    private static bool IsPortableMode()
    {
        string exeDir = AppContext.BaseDirectory;
        string portableMarker = Path.Combine(exeDir, "portable.txt");
        string settingsFile = Path.Combine(exeDir, "settings.json");
        
        return File.Exists(portableMarker) || File.Exists(settingsFile);
    }

    public AppSettings Load()
    {
        if (!File.Exists(SettingsFilePath))
        {
            return new AppSettings();
        }

        try
        {
            string json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsDirectoryPath);
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsFilePath, json);
    }
}
