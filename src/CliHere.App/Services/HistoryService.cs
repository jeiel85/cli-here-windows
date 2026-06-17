using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class HistoryService
{
    private readonly SettingsService _settingsService;
    private const int MaxHistoryItems = 50;

    public HistoryService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IReadOnlyList<CliHistoryEntry> GetAll()
    {
        AppSettings settings = _settingsService.Load();
        return settings.History.OrderByDescending(h => h.ExecutedAt).ToList();
    }

    public IReadOnlyList<CliHistoryEntry> GetRecent(int count)
    {
        return GetAll().Take(count).ToList();
    }

    public void Add(string cliId, string folderPath, TerminalMode terminalMode, bool runAsAdministrator)
    {
        AppSettings settings = _settingsService.Load();

        CliHistoryEntry entry = new()
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            CliId = cliId,
            FolderPath = folderPath,
            TerminalMode = terminalMode,
            RunAsAdministrator = runAsAdministrator,
            ExecutedAt = DateTime.Now,
        };

        settings.History.Insert(0, entry);

        if (settings.History.Count > MaxHistoryItems)
        {
            settings.History = settings.History.Take(MaxHistoryItems).ToList();
        }

        _settingsService.Save(settings);
    }

    public void Clear()
    {
        AppSettings settings = _settingsService.Load();
        settings.History.Clear();
        _settingsService.Save(settings);
    }

    public void Delete(string entryId)
    {
        AppSettings settings = _settingsService.Load();
        settings.History.RemoveAll(h => h.Id == entryId);
        _settingsService.Save(settings);
    }
}
