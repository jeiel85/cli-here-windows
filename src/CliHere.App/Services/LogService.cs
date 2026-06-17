using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class LogService
{
    private readonly SettingsService _settingsService;
    private const int MaxLogItems = 100;

    public LogService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IReadOnlyList<CliLogEntry> GetAll()
    {
        AppSettings settings = _settingsService.Load();
        return settings.Logs.OrderByDescending(l => l.ExecutedAt).ToList();
    }

    public IReadOnlyList<CliLogEntry> GetByCli(string cliId)
    {
        return GetAll().Where(l => l.CliId == cliId).ToList();
    }

    public void Add(string cliId, string folderPath, string command, TimeSpan duration, int exitCode = 0, string? errorMessage = null)
    {
        AppSettings settings = _settingsService.Load();

        CliLogEntry entry = new()
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            CliId = cliId,
            FolderPath = folderPath,
            Command = command,
            ExecutedAt = DateTime.Now,
            Duration = duration,
            ExitCode = exitCode,
            ErrorMessage = errorMessage,
        };

        settings.Logs.Insert(0, entry);

        if (settings.Logs.Count > MaxLogItems)
        {
            settings.Logs = settings.Logs.Take(MaxLogItems).ToList();
        }

        _settingsService.Save(settings);
    }

    public void Clear()
    {
        AppSettings settings = _settingsService.Load();
        settings.Logs.Clear();
        _settingsService.Save(settings);
    }
}
