using System.Diagnostics;
using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class TerminalLauncher : ITerminalLauncher
{
    private static readonly string[] PowerShellCommandExtensions = [".cmd", ".exe", ".bat", ".com"];

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
        bool useCmd = terminalMode == TerminalMode.Cmd;
        bool useGitBash = terminalMode == TerminalMode.GitBash && IsGitBashAvailable();

        string fileName;
        string arguments;

        if (useWindowsTerminal)
        {
            string shellExecutable = usePowerShell7 ? "pwsh.exe" : "powershell.exe";
            string powerShellCommand = ResolvePowerShellCommand(command);
            fileName = "wt.exe";
            arguments = $"-d \"{workingDirectory}\" {shellExecutable} -ExecutionPolicy Bypass -NoExit -Command \"{powerShellCommand}\"";
        }
        else if (useCmd)
        {
            fileName = "cmd.exe";
            arguments = $"/k cd /d \"{workingDirectory}\" && {command}";
        }
        else if (useGitBash)
        {
            string gitBashPath = FindGitBashPath() ?? "bash.exe";
            fileName = gitBashPath;
            arguments = $"--cd \"{workingDirectory}\" -c \"{command}\"";
        }
        else
        {
            string shellExecutable = usePowerShell7 ? "pwsh.exe" : "powershell.exe";
            string powerShellCommand = ResolvePowerShellCommand(command);
            fileName = shellExecutable;
            arguments = $"-ExecutionPolicy Bypass -NoExit -Command \"{powerShellCommand}\"";
        }

        return new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = runAsAdministrator,
            Verb = runAsAdministrator ? "runas" : string.Empty,
        };
    }

    internal static bool IsGitBashAvailable() => FindGitBashPath() is not null;

    private static string? FindGitBashPath()
    {
        string[] candidates =
        [
            @"C:\Program Files\Git\bin\bash.exe",
            @"C:\Program Files (x86)\Git\bin\bash.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Git", "bin", "bash.exe"),
        ];

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    internal static string Quote(string value)
    {
        return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }

    internal static string ResolvePowerShellCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command) ||
            command.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            command.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal))
        {
            return command;
        }

        string? resolvedCommand = ResolveCommandPath(command);
        if (string.IsNullOrWhiteSpace(resolvedCommand))
        {
            return command;
        }

        return $"& {QuotePowerShellLiteral(resolvedCommand)}";
    }

    private static string? ResolveCommandPath(string command)
    {
        if (Path.HasExtension(command))
        {
            return ResolveCommandPath(command, [string.Empty]);
        }

        return ResolveCommandPath(command, PowerShellCommandExtensions);
    }

    private static string? ResolveCommandPath(string command, IReadOnlyList<string> extensions)
    {
        foreach (string pathSegment in EnumeratePathSegments())
        {
            foreach (string extension in extensions)
            {
                string candidate = Path.Combine(pathSegment, command + extension);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    internal static string QuotePowerShellLiteral(string value)
    {
        return $"'{value.Replace("'", "''", StringComparison.Ordinal)}'";
    }

    internal static bool IsWindowsTerminalAvailable() => IsCommandAvailable("wt.exe");

    internal static bool IsCommandAvailable(string fileName)
    {
        foreach (string pathSegment in EnumeratePathSegments())
        {
            if (File.Exists(Path.Combine(pathSegment, fileName)))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> EnumeratePathSegments()
    {
        string? pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            yield break;
        }

        foreach (string pathSegment in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string normalized = Environment.ExpandEnvironmentVariables(pathSegment.Trim().Trim('"'));
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                yield return normalized;
            }
        }
    }
}
