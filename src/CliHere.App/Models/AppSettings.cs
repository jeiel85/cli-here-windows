namespace CliHere.App.Models;

public enum LanguageMode
{
    System,
    Korean,
    English,
}

public enum TerminalMode
{
    WindowsTerminal,
    PowerShell,
    PowerShell7,
    Cmd,
    GitBash,
}

public sealed class CliProfile
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string CliId { get; init; }
    public string? WorkingDirectory { get; init; }
    public TerminalMode? TerminalMode { get; init; }
    public bool? RunAsAdministrator { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime LastUsedAt { get; set; } = DateTime.Now;
    public int UseCount { get; set; }
}

public sealed class AppSettings
{
    public LanguageMode Language { get; set; } = LanguageMode.System;
    public TerminalMode Terminal { get; set; } = TerminalMode.WindowsTerminal;
    public bool RunAsAdministrator { get; set; }
    public List<string> EnabledCliIds { get; set; } = [];
    public string? SkippedUpdateVersion { get; set; }
    public List<CustomCliDefinition> CustomCliDefinitions { get; set; } = [];
    public List<CliProfile> Profiles { get; set; } = [];
    public string? LastSelectedProfileId { get; set; }
}
