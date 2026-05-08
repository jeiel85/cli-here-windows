namespace CliHere.App.Models;

public sealed class CustomCliDefinition
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ExecutableName { get; set; } = string.Empty;
    public string InstallUrl { get; set; } = string.Empty;
    public string DocsUrl { get; set; } = string.Empty;

    public CliDefinition ToCliDefinition()
    {
        return new CliDefinition
        {
            Id = Id,
            DisplayName = DisplayName,
            ExecutableName = ExecutableName,
            InstallUrl = InstallUrl,
            DocsUrl = DocsUrl,
        };
    }
}
