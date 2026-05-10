using System.Diagnostics;
using CliHere.App.Models;
using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class TerminalLauncherTests
{
    [Fact]
    public void Quote_EscapesEmbeddedDoubleQuotes()
    {
        string raw = "C:\\Work Folder\\\"My Project\"";
        string quoted = TerminalLauncher.Quote(raw);

        Assert.StartsWith("\"", quoted, StringComparison.Ordinal);
        Assert.EndsWith("\"", quoted, StringComparison.Ordinal);
        Assert.Contains("\\\"", quoted, StringComparison.Ordinal);
    }

    [Fact]
    public void QuotePowerShellLiteral_EscapesEmbeddedSingleQuotes()
    {
        string quoted = TerminalLauncher.QuotePowerShellLiteral("C:\\Work's Folder\\codex.cmd");

        Assert.Equal("'C:\\Work''s Folder\\codex.cmd'", quoted);
    }

    [Fact]
    public void CreateStartInfo_WhenRunAsAdministrator_SetsRunAsAndShellExecute()
    {
        using PathScope pathScope = PathScope.CreateEmpty();

        ProcessStartInfo psi = TerminalLauncher.CreateStartInfo(
            TerminalMode.PowerShell,
            runAsAdministrator: true,
            workingDirectory: "C:\\Work Folder",
            command: "codex");

        Assert.Equal("powershell.exe", psi.FileName);
        Assert.True(psi.UseShellExecute);
        Assert.Equal("runas", psi.Verb);
        Assert.Contains("-NoExit -Command \"codex\"", psi.Arguments, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateStartInfo_WhenPowerShell7Selected_UsesAvailableShell()
    {
        using PathScope pathScope = PathScope.CreateEmpty();

        ProcessStartInfo psi = TerminalLauncher.CreateStartInfo(
            TerminalMode.PowerShell7,
            runAsAdministrator: false,
            workingDirectory: "C:\\Work Folder",
            command: "codex");

        Assert.True(psi.FileName is "pwsh.exe" or "powershell.exe");
        Assert.Contains("-NoExit -Command \"codex\"", psi.Arguments, StringComparison.Ordinal);
    }

    [Fact]
    public void ResolvePowerShellCommand_WhenCmdShimExists_PrefersCmdOverPowerShellScript()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string cmdPath = Path.Combine(tempDir, "codex.cmd");
        string ps1Path = Path.Combine(tempDir, "codex.ps1");
        File.WriteAllText(cmdPath, "@echo off");
        File.WriteAllText(ps1Path, "Write-Host codex");

        using PathScope pathScope = new(tempDir);

        string resolved = TerminalLauncher.ResolvePowerShellCommand("codex");

        Assert.Equal($"& {TerminalLauncher.QuotePowerShellLiteral(cmdPath)}", resolved);
    }

    [Fact]
    public void CreateStartInfo_WhenCmdShimExists_UsesResolvedCmdPath()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string cmdPath = Path.Combine(tempDir, "codex.cmd");
        File.WriteAllText(cmdPath, "@echo off");

        using PathScope pathScope = new(tempDir);

        ProcessStartInfo psi = TerminalLauncher.CreateStartInfo(
            TerminalMode.PowerShell,
            runAsAdministrator: false,
            workingDirectory: "C:\\Work Folder",
            command: "codex");

        Assert.Contains($"-NoExit -Command \"& '{cmdPath}'\"", psi.Arguments, StringComparison.Ordinal);
    }

    [Fact]
    public void IsCommandAvailable_UsesRequestedFileName()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "CliHere.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "pwsh.exe"), string.Empty);

        using PathScope pathScope = new(tempDir);

        Assert.True(TerminalLauncher.IsCommandAvailable("pwsh.exe"));
        Assert.False(TerminalLauncher.IsCommandAvailable("wt.exe"));
    }

    private sealed class PathScope : IDisposable
    {
        private readonly string? _originalPath;

        public PathScope(string path)
        {
            _originalPath = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", path);
        }

        public static PathScope CreateEmpty() => new(string.Empty);

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("PATH", _originalPath);
        }
    }
}
