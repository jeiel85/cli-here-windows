using CliHere.App.Services;
using CliHere.App.Models;

namespace CliHere.Tests;

public sealed class CliDefinitionServiceTests
{
    [Fact]
    public void GetAll_IncludesExpectedBuiltInCliIds()
    {
        CliDefinitionService service = new();
        var ids = service.GetAll().Select(x => x.Id).ToArray();

        Assert.Contains("agy", ids);
        Assert.Contains("opencode", ids);
        Assert.Contains("claude", ids);
        Assert.Contains("codex", ids);
        Assert.Contains("cursor", ids);
        Assert.Contains("windsurf", ids);
        Assert.Contains("aider", ids);
        Assert.Contains("continue", ids);
        Assert.Contains("cline", ids);
    }

    [Fact]
    public void GetAll_IncludesCustomCliFromSettings()
    {
        CliDefinitionService service = new();
        AppSettings settings = new()
        {
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
        };

        var ids = service.GetAll(settings).Select(x => x.Id).ToArray();
        Assert.Contains("custom-foo", ids);
    }
}
