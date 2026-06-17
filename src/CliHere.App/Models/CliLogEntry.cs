namespace CliHere.App.Models;

public sealed class CliLogEntry
{
    public required string Id { get; init; }
    public required string CliId { get; init; }
    public required string FolderPath { get; init; }
    public required string Command { get; init; }
    public required DateTime ExecutedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public int ExitCode { get; init; }
    public TimeSpan Duration { get; init; }
}
