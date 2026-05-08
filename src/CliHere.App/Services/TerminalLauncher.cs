using System.Diagnostics;
using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class TerminalLauncher : ITerminalLauncher
{
    public void Launch(TerminalMode terminalMode, bool runAsAdministrator, string workingDirectory, string command)
    {
        ProcessStartInfo processStartInfo = CreateStartInfo(terminalMode, runAsAdministrator, workingDirectory, command);

        try
        {
            Process.Start(processStartInfo);
        }
        catch (System.ComponentModel.Win32Exception ex) when (runAsAdministrator && ex.NativeErrorCode == 1223)
        {
            throw new OperationCanceledException("User canceled UAC prompt.", ex);
        }
    }

    internal static ProcessStartInfo CreateStartInfo(TerminalMode terminalMode, bool runAsAdministrator, string workingDirectory, string command)
    {
        bool useWindowsTerminal = terminalMode == TerminalMode.WindowsTerminal && IsWindowsTerminalAvailable();
        bool usePowerShell7 = terminalMode == TerminalMode.PowerShell7 && IsCommandAvailable("pwsh.exe");
        string shellExecutable = usePowerShell7 ? "pwsh.exe" : "powershell.exe";
        string fileName = useWindowsTerminal ? "wt.exe" : shellExecutable;
        string arguments = useWindowsTerminal
            ? $"-d {Quote(workingDirectory)} {shellExecutable} -NoExit -Command {Quote(command)}"
            : $"-NoExit -Command {Quote(command)}";

        return new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = runAsAdministrator,
            Verb = runAsAdministrator ? "runas" : string.Empty,
        };
    }

    internal static string Quote(string value)
    {
        return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }

    internal static bool IsWindowsTerminalAvailable() => IsCommandAvailable("wt.exe");

    internal static bool IsCommandAvailable(string fileName)
    {
        string? pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return false;
        }

        foreach (string pathSegment in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (File.Exists(Path.Combine(pathSegment, "wt.exe")))
            {
                return true;
            }
        }

        return false;
    }
}
