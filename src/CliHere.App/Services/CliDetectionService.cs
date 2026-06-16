using System.Diagnostics;
using CliHere.App.Models;
using Microsoft.Win32;

namespace CliHere.App.Services;

public sealed class CliDetectionService
{
    public CliDetectionResult Detect(CliDefinition definition)
    {
        string? resolvedPath = ResolveExecutablePath(definition.ExecutableName);
        string? version = null;
        
        if (!string.IsNullOrWhiteSpace(resolvedPath))
        {
            version = DetectVersion(resolvedPath);
        }

        return new CliDetectionResult
        {
            CliId = definition.Id,
            IsInstalled = !string.IsNullOrWhiteSpace(resolvedPath),
            ResolvedPath = resolvedPath,
            Version = version,
        };
    }

    private static string? DetectVersion(string executablePath)
    {
        try
        {
            string? directory = Path.GetDirectoryName(executablePath);
            if (string.IsNullOrWhiteSpace(directory)) return null;

            string[] versionFiles = ["--version", "-v", "-V", "version"];
            string[] extensions = ["", ".cmd", ".bat", ".exe"];

            foreach (string versionArg in versionFiles)
            {
                foreach (string extension in extensions)
                {
                    string candidate = Path.Combine(directory, Path.GetFileNameWithoutExtension(executablePath) + extension);
                    if (!File.Exists(candidate)) continue;

                    try
                    {
                        ProcessStartInfo psi = new()
                        {
                            FileName = candidate,
                            Arguments = versionArg,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WorkingDirectory = directory,
                        };

                        using Process process = Process.Start(psi);
                        if (process is null) continue;

                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit(2000);

                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            string version = ParseVersionFromOutput(output);
                            if (!string.IsNullOrWhiteSpace(version))
                            {
                                return version;
                            }
                        }
                    }
                    catch
                    {
                        // Version detection is best-effort
                    }
                }
            }
        }
        catch
        {
            // Version detection is best-effort
        }

        return null;
    }

    private static string ParseVersionFromOutput(string output)
    {
        string[] lines = output.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            var match = System.Text.RegularExpressions.Regex.Match(trimmed, @"(\d+\.\d+(?:\.\d+)*)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        return string.Empty;
    }

    private static string? ResolveExecutablePath(string executableName)
    {
        string[] pathExtensions = (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string segment in EnumerateSearchPaths())
        {
            if (File.Exists(Path.Combine(segment, executableName)))
            {
                return Path.Combine(segment, executableName);
            }

            foreach (string extension in pathExtensions)
            {
                string ext = extension.StartsWith('.') ? extension : $".{extension}";
                string candidate = Path.Combine(segment, executableName + ext.ToLowerInvariant());
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateSearchPaths()
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        foreach (string segment in EnumerateProcessPath()) if (seen.Add(segment)) yield return segment;
        foreach (string segment in EnumerateRegistryPath(RegistryHive.CurrentUser, "Environment")) if (seen.Add(segment)) yield return segment;
        foreach (string segment in EnumerateRegistryPath(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment")) if (seen.Add(segment)) yield return segment;
        foreach (string segment in EnumerateWellKnownPaths()) if (seen.Add(segment)) yield return segment;
    }

    private static IEnumerable<string> EnumerateProcessPath()
    {
        string? pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue)) yield break;

        foreach (string raw in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string seg = NormalizePathSegment(raw);
            if (!string.IsNullOrWhiteSpace(seg)) yield return seg;
        }
    }

    private static IEnumerable<string> EnumerateRegistryPath(RegistryHive hive, string subKey)
    {
        if (!OperatingSystem.IsWindows()) yield break;

        string? raw = null;
        try
        {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using RegistryKey? key = baseKey.OpenSubKey(subKey);
            raw = key?.GetValue("Path", null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
        }
        catch
        {
            // Registry access can fail under reduced privilege; treat as no contribution.
        }

        if (string.IsNullOrWhiteSpace(raw)) yield break;

        foreach (string segment in raw.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string seg = NormalizePathSegment(segment);
            if (!string.IsNullOrWhiteSpace(seg)) yield return seg;
        }
    }

    private static IEnumerable<string> EnumerateWellKnownPaths()
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string[] candidates =
        [
            Path.Combine(roaming, "npm"),
            Path.Combine(userProfile, ".local", "bin"),
            Path.Combine(userProfile, ".cargo", "bin"),
            Path.Combine(userProfile, ".dotnet", "tools"),
            Path.Combine(userProfile, ".bun", "bin"),
            Path.Combine(localApp, "Programs", "opencode"),
            Path.Combine(localApp, "Programs", "cursor"),
            Path.Combine(userProfile, ".cursor", "bin"),
            Path.Combine(userProfile, ".windsurf", "bin"),
            Path.Combine(userProfile, ".aider", "bin"),
            Path.Combine(userProfile, ".continue", "bin"),
            Path.Combine(userProfile, ".cline", "bin"),
            Path.Combine(roaming, "Python", "Scripts"),
            Path.Combine(localApp, "agy", "bin"),
        ];

        foreach (string candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && Directory.Exists(candidate))
            {
                yield return candidate;
            }
        }
    }

    private static string NormalizePathSegment(string rawPathSegment)
    {
        string cleaned = rawPathSegment.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return string.Empty;
        }

        return Environment.ExpandEnvironmentVariables(cleaned);
    }
}
