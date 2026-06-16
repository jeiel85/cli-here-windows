using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class ProfileService
{
    private readonly SettingsService _settingsService;

    public ProfileService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IReadOnlyList<CliProfile> GetAll()
    {
        AppSettings settings = _settingsService.Load();
        return settings.Profiles.OrderByDescending(p => p.LastUsedAt).ToList();
    }

    public CliProfile? GetById(string profileId)
    {
        AppSettings settings = _settingsService.Load();
        return settings.Profiles.FirstOrDefault(p => p.Id == profileId);
    }

    public CliProfile Create(string name, string cliId, string? workingDirectory = null, TerminalMode? terminalMode = null, bool? runAsAdministrator = null)
    {
        AppSettings settings = _settingsService.Load();
        
        CliProfile profile = new()
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Name = name,
            CliId = cliId,
            WorkingDirectory = workingDirectory,
            TerminalMode = terminalMode,
            RunAsAdministrator = runAsAdministrator,
            CreatedAt = DateTime.Now,
            LastUsedAt = DateTime.Now,
            UseCount = 0,
        };

        settings.Profiles.Add(profile);
        _settingsService.Save(settings);
        return profile;
    }

    public void Update(string profileId, Action<CliProfile> updater)
    {
        AppSettings settings = _settingsService.Load();
        CliProfile? profile = settings.Profiles.FirstOrDefault(p => p.Id == profileId);
        if (profile is null) return;

        updater(profile);
        _settingsService.Save(settings);
    }

    public void Delete(string profileId)
    {
        AppSettings settings = _settingsService.Load();
        settings.Profiles.RemoveAll(p => p.Id == profileId);
        _settingsService.Save(settings);
    }

    public void RecordUsage(string profileId)
    {
        AppSettings settings = _settingsService.Load();
        CliProfile? profile = settings.Profiles.FirstOrDefault(p => p.Id == profileId);
        if (profile is null) return;

        profile.LastUsedAt = DateTime.Now;
        profile.UseCount++;
        _settingsService.Save(settings);
    }

    public string? GetLastSelectedProfileId()
    {
        AppSettings settings = _settingsService.Load();
        return settings.LastSelectedProfileId;
    }

    public void SetLastSelectedProfileId(string? profileId)
    {
        AppSettings settings = _settingsService.Load();
        settings.LastSelectedProfileId = profileId;
        _settingsService.Save(settings);
    }
}
