using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class GroupService
{
    private readonly SettingsService _settingsService;

    public GroupService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IReadOnlyList<CliGroup> GetAll()
    {
        AppSettings settings = _settingsService.Load();
        return settings.Groups;
    }

    public CliGroup? GetById(string groupId)
    {
        AppSettings settings = _settingsService.Load();
        return settings.Groups.FirstOrDefault(g => g.Id == groupId);
    }

    public CliGroup Create(string name)
    {
        AppSettings settings = _settingsService.Load();

        CliGroup group = new()
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Name = name,
        };

        settings.Groups.Add(group);
        _settingsService.Save(settings);
        return group;
    }

    public void Update(string groupId, Action<CliGroup> updater)
    {
        AppSettings settings = _settingsService.Load();
        CliGroup? group = settings.Groups.FirstOrDefault(g => g.Id == groupId);
        if (group is null) return;

        updater(group);
        _settingsService.Save(settings);
    }

    public void Delete(string groupId)
    {
        AppSettings settings = _settingsService.Load();
        settings.Groups.RemoveAll(g => g.Id == groupId);
        _settingsService.Save(settings);
    }

    public void AddCliToGroup(string groupId, string cliId)
    {
        AppSettings settings = _settingsService.Load();
        CliGroup? group = settings.Groups.FirstOrDefault(g => g.Id == groupId);
        if (group is null) return;

        if (!group.CliIds.Contains(cliId))
        {
            group.CliIds.Add(cliId);
            _settingsService.Save(settings);
        }
    }

    public void RemoveCliFromGroup(string groupId, string cliId)
    {
        AppSettings settings = _settingsService.Load();
        CliGroup? group = settings.Groups.FirstOrDefault(g => g.Id == groupId);
        if (group is null) return;

        group.CliIds.Remove(cliId);
        _settingsService.Save(settings);
    }
}
