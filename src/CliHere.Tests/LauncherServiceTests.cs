using CliHere.App.Models;
using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class LauncherServiceTests
{
    [Fact]
    public void RunCli_WhenCliIdIsEmpty_ThrowsArgumentException()
    {
        LauncherService service = CreateService(out _);
        string folderPath = Path.GetTempPath();

        Assert.Throws<ArgumentException>(() => service.RunCli("", folderPath));
    }

    [Fact]
    public void RunCli_WhenFolderPathIsInvalid_ThrowsArgumentException()
    {
        LauncherService service = CreateService(out _);
        Assert.Throws<ArgumentException>(() => service.RunCli("codex", "Z:\\does-not-exist\\folder"));
    }

    [Fact]
    public void RunCli_WhenValid_CallsTerminalLauncher()
    {
        LauncherService service = CreateService(out FakeTerminalLauncher fakeLauncher);
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        service.RunCli("codex", tempDir);

        Assert.Equal("codex", fakeLauncher.LastCommand);
        Assert.Equal(tempDir, fakeLauncher.LastWorkingDirectory);
    }

    [Fact]
    public void RunCli_WhenCustomCliId_CallsTerminalLauncherWithCustomExecutable()
    {
        FakeTerminalLauncher fakeLauncher;
        SettingsService settingsService = new(Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N")));
        settingsService.Save(new AppSettings
        {
            Terminal = TerminalMode.PowerShell,
            CustomCliDefinitions =
            [
                new CustomCliDefinition
                {
                    Id = "custom-foo",
                    DisplayName = "Foo CLI",
                    ExecutableName = "foo",
                    InstallUrl = "https://example.com/install",
                    DocsUrl = "https://example.com/docs",
                },
            ],
        });

        LauncherService service = new(new CliDefinitionService(), settingsService, fakeLauncher = new FakeTerminalLauncher());
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        service.RunCli("custom-foo", tempDir);
        Assert.Equal("foo", fakeLauncher.LastCommand);
    }

    [Theory]
    [InlineData(TerminalMode.WindowsTerminal)]
    [InlineData(TerminalMode.PowerShell)]
    [InlineData(TerminalMode.PowerShell7)]
    [InlineData(TerminalMode.Cmd)]
    [InlineData(TerminalMode.GitBash)]
    public void RunCli_WithAllTerminalModes_CallsTerminalLauncher(TerminalMode mode)
    {
        FakeTerminalLauncher fakeLauncher;
        SettingsService settingsService = new(Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N")));
        settingsService.Save(new AppSettings { Terminal = mode });

        LauncherService service = new(new CliDefinitionService(), settingsService, fakeLauncher = new FakeTerminalLauncher());
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        service.RunCli("claude", tempDir);

        Assert.Equal(mode, fakeLauncher.LastTerminalMode);
        Assert.Equal("claude", fakeLauncher.LastCommand);
    }

    private static LauncherService CreateService(out FakeTerminalLauncher fakeLauncher)
    {
        fakeLauncher = new FakeTerminalLauncher();
        SettingsService settingsService = new(Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N")));
        settingsService.Save(new AppSettings
        {
            Terminal = TerminalMode.PowerShell,
            RunAsAdministrator = false,
        });

        return new LauncherService(new CliDefinitionService(), settingsService, fakeLauncher);
    }

    private sealed class FakeTerminalLauncher : ITerminalLauncher
    {
        public string? LastWorkingDirectory { get; private set; }
        public string? LastCommand { get; private set; }
        public TerminalMode LastTerminalMode { get; private set; }
        public bool LastRunAsAdministrator { get; private set; }

        public void Launch(TerminalMode terminalMode, bool runAsAdministrator, string workingDirectory, string command)
        {
            LastTerminalMode = terminalMode;
            LastRunAsAdministrator = runAsAdministrator;
            LastWorkingDirectory = workingDirectory;
            LastCommand = command;
        }
    }
}
