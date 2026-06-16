using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class ContextMenuRegistryServiceTests
{
    [Fact]
    public void BuildOwnedKeyName_UsesCliHerePrefix()
    {
        string key = ContextMenuRegistryService.BuildOwnedKeyName("codex");
        Assert.Equal("CliHere_codex", key);
    }

    [Fact]
    public void IsOwnedKey_ReturnsTrueOnlyForOwnedPrefix()
    {
        Assert.True(ContextMenuRegistryService.IsOwnedKey("CliHere_gemini"));
        Assert.True(ContextMenuRegistryService.IsOwnedKey(ContextMenuRegistryService.ParentGroupKey));
        Assert.False(ContextMenuRegistryService.IsOwnedKey("OtherApp_gemini"));
    }

    [Fact]
    public void BuildLauncherCommand_FormatsExpectedRunSyntax()
    {
        string command = ContextMenuRegistryService.BuildLauncherCommand(
            @"C:\Program Files\CliHere\CliHere.exe",
            "claude",
            "%V");

        Assert.Equal("\"C:\\Program Files\\CliHere\\CliHere.exe\" run claude \"%V\"", command);
    }

    [Theory]
    [InlineData("agy")]
    [InlineData("cursor")]
    [InlineData("windsurf")]
    [InlineData("aider")]
    [InlineData("continue")]
    [InlineData("cline")]
    public void BuildOwnedKeyName_WorksForAllBuiltInCliIds(string cliId)
    {
        string key = ContextMenuRegistryService.BuildOwnedKeyName(cliId);
        Assert.Equal($"CliHere_{cliId}", key);
    }

    [Theory]
    [InlineData("CliHere_agy")]
    [InlineData("CliHere_cursor")]
    [InlineData("CliHere_windsurf")]
    [InlineData("CliHere_aider")]
    [InlineData("CliHere_continue")]
    [InlineData("CliHere_cline")]
    public void IsOwnedKey_ReturnsTrueForNewCliIds(string keyName)
    {
        Assert.True(ContextMenuRegistryService.IsOwnedKey(keyName));
    }

    [Theory]
    [InlineData("CustomApp_gemini")]
    [InlineData("Other_cli")]
    [InlineData("NotOwned")]
    public void IsOwnedKey_ReturnsFalseForNonOwnedKeys(string keyName)
    {
        Assert.False(ContextMenuRegistryService.IsOwnedKey(keyName));
    }
}
