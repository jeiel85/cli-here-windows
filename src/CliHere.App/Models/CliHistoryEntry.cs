namespace CliHere.App.Models;

public sealed class CliHistoryEntry
{
    public required string Id { get; init; }
    public required string CliId { get; init; }
    public required string FolderPath { get; init; }
    public required TerminalMode TerminalMode { get; init; }
    public required bool RunAsAdministrator { get; init; }
    public required DateTime ExecutedAt { get; init; }
}
