namespace CliHere.App.Models;

public sealed class CliGroup
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public List<string> CliIds { get; set; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.Now;
}
