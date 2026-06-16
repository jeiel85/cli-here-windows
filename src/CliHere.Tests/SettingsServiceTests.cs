using CliHere.App.Models;
using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class SettingsServiceTests
{
    [Fact]
    public void Load_WhenFileMissing_ReturnsDefaults()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        SettingsService service = new(tempDir);
        AppSettings settings = service.Load();

        Assert.Equal(LanguageMode.System, settings.Language);
        Assert.Equal(TerminalMode.WindowsTerminal, settings.Terminal);
        Assert.False(settings.RunAsAdministrator);
    }

    [Fact]
    public void SaveAndLoad_PersistsSkippedUpdateVersion()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        SettingsService service = new(tempDir);
        AppSettings saved = new()
        {
            Language = LanguageMode.English,
            Terminal = TerminalMode.PowerShell,
            RunAsAdministrator = true,
            EnabledCliIds = ["codex"],
            SkippedUpdateVersion = "0.1.1",
        };

        service.Save(saved);
        AppSettings loaded = service.Load();

        Assert.Equal(LanguageMode.English, loaded.Language);
        Assert.Equal(TerminalMode.PowerShell, loaded.Terminal);
        Assert.True(loaded.RunAsAdministrator);
        Assert.Single(loaded.EnabledCliIds);
        Assert.Equal("0.1.1", loaded.SkippedUpdateVersion);
    }

    [Fact]
    public void Load_WhenJsonIsInvalid_ReturnsDefaults()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "settings.json"), "{ not-valid-json");

        SettingsService service = new(tempDir);
        AppSettings loaded = service.Load();

        Assert.Equal(LanguageMode.System, loaded.Language);
        Assert.Equal(TerminalMode.WindowsTerminal, loaded.Terminal);
        Assert.False(loaded.RunAsAdministrator);
        Assert.Null(loaded.SkippedUpdateVersion);
    }

    [Fact]
    public void SettingsService_WhenCustomPath_IsPortable()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        SettingsService service = new(tempDir);

        Assert.True(service.IsPortable);
        Assert.Equal(tempDir, service.SettingsDirectoryPath);
    }

    [Fact]
    public void SaveAndLoad_WithNewTerminalModes_PersistsCorrectly()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        SettingsService service = new(tempDir);
        
        AppSettings saved = new()
        {
            Terminal = TerminalMode.Cmd,
        };
        service.Save(saved);
        AppSettings loaded = service.Load();
        
        Assert.Equal(TerminalMode.Cmd, loaded.Terminal);
    }

    [Theory]
    [InlineData(TerminalMode.WindowsTerminal)]
    [InlineData(TerminalMode.PowerShell)]
    [InlineData(TerminalMode.PowerShell7)]
    [InlineData(TerminalMode.Cmd)]
    [InlineData(TerminalMode.GitBash)]
    public void SaveAndLoad_AllTerminalModes_PersistsCorrectly(TerminalMode mode)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        SettingsService service = new(tempDir);
        
        AppSettings saved = new() { Terminal = mode };
        service.Save(saved);
        AppSettings loaded = service.Load();
        
        Assert.Equal(mode, loaded.Terminal);
    }
}
