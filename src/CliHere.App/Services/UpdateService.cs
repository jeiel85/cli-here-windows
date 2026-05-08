using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace CliHere.App.Services;

public enum UpdateCheckErrorKind
{
    Unknown,
    Network,
    Timeout,
    RateLimit,
    ApiError,
}

public sealed class UpdateCheckException : Exception
{
    public UpdateCheckErrorKind Kind { get; }
    public int? StatusCode { get; }
    public string? RetryAtLocal { get; }

    public UpdateCheckException(
        string message,
        UpdateCheckErrorKind kind,
        int? statusCode = null,
        string? retryAtLocal = null,
        Exception? inner = null) : base(message, inner)
    {
        Kind = kind;
        StatusCode = statusCode;
        RetryAtLocal = retryAtLocal;
    }
}

public sealed class UpdateService
{
    private const string Repo = "jeiel85/cli-here";
    private const string AssetExeName = "CliHere.exe";
    private const string ProcessName = "CliHere";
    private const string UserAgent = "CliHere-Updater";

    private const string ApiListUrl = $"https://api.github.com/repos/{Repo}/releases?per_page=30";
    public const string ReleasePage = $"https://github.com/{Repo}/releases/latest";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public record UpdateInfo(Version Version, string DownloadUrl, string Sha256Url, string ReleaseNotes);

    static UpdateService()
    {
        Http.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    public static Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

    public async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        HttpResponseMessage response;
        string rawJson;
        try
        {
            response = await Http.GetAsync(ApiListUrl);
            rawJson = await response.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException ex)
        {
            throw new UpdateCheckException(ex.Message, UpdateCheckErrorKind.Timeout, inner: ex);
        }
        catch (HttpRequestException ex)
        {
            throw new UpdateCheckException(ex.Message, UpdateCheckErrorKind.Network, inner: ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            int code = (int)response.StatusCode;
            bool isRateLimit = code == 403 &&
                               (rawJson.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
                                (response.Headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? remVals) &&
                                 remVals.FirstOrDefault() == "0"));

            if (isRateLimit)
            {
                string? retryAt = null;
                if (response.Headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? resetVals) &&
                    long.TryParse(resetVals.FirstOrDefault(), out long epoch))
                {
                    retryAt = DateTimeOffset.FromUnixTimeSeconds(epoch).ToLocalTime().ToString("HH:mm");
                }
                else if (response.Headers.TryGetValues("Retry-After", out IEnumerable<string>? raVals) &&
                         int.TryParse(raVals.FirstOrDefault(), out int raSec))
                {
                    retryAt = DateTimeOffset.UtcNow.AddSeconds(raSec).ToLocalTime().ToString("HH:mm");
                }

                throw new UpdateCheckException("GitHub API rate limit exceeded.", UpdateCheckErrorKind.RateLimit, code, retryAt);
            }

            throw new UpdateCheckException($"GitHub API returned HTTP {code}.", UpdateCheckErrorKind.ApiError, code);
        }

        using JsonDocument doc = JsonDocument.Parse(rawJson);
        JsonElement root = doc.RootElement;
        List<(Version version, string tag, string body, JsonElement element)> newer = [];

        foreach (JsonElement rel in root.EnumerateArray())
        {
            if (rel.TryGetProperty("draft", out JsonElement draft) && draft.GetBoolean()) continue;
            if (rel.TryGetProperty("prerelease", out JsonElement pre) && pre.GetBoolean()) continue;

            string tag = rel.TryGetProperty("tag_name", out JsonElement tagEl) ? tagEl.GetString() ?? "" : "";
            if (!Version.TryParse(tag.TrimStart('v'), out Version? version)) continue;
            if (version <= CurrentVersion) continue;

            string body = rel.TryGetProperty("body", out JsonElement bodyEl) ? bodyEl.GetString() ?? "" : "";
            newer.Add((version, tag, body, rel));
        }

        if (newer.Count == 0) return null;
        newer.Sort((a, b) => b.version.CompareTo(a.version));
        (Version version, string tag, string body, JsonElement element) latest = newer[0];

        StringBuilder notesBuilder = new();
        foreach ((Version _, string tag, string body, JsonElement _) in newer)
        {
            if (string.IsNullOrWhiteSpace(body)) continue;
            if (notesBuilder.Length > 0) notesBuilder.AppendLine();
            notesBuilder.AppendLine($"## {tag}");
            notesBuilder.Append(body);
        }

        string? exeUrl = null;
        string? shaUrl = null;
        foreach (JsonElement asset in latest.element.GetProperty("assets").EnumerateArray())
        {
            string name = asset.GetProperty("name").GetString() ?? "";
            string url = asset.GetProperty("browser_download_url").GetString() ?? "";
            if (name.Equals(AssetExeName, StringComparison.OrdinalIgnoreCase)) exeUrl = url;
            if (name.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase) || name.Equals("SHA256.txt", StringComparison.OrdinalIgnoreCase)) shaUrl = url;
        }

        if (exeUrl is null) return null;
        return new UpdateInfo(latest.version, exeUrl, shaUrl ?? "", notesBuilder.ToString().TrimEnd());
    }

    public async Task<string> DownloadAndPrepareUpdateAsync(string downloadUrl, string sha256Url, Action<int, string> onProgress)
    {
        string tempExe = Path.Combine(Path.GetTempPath(), $"{ProcessName}_new_{Guid.NewGuid():N}.exe");
        onProgress(0, "Downloading...");

        using (HttpResponseMessage response = await Http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            long totalBytes = response.Content.Headers.ContentLength ?? 0;

            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = new(tempExe, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            byte[] buffer = new byte[8192];
            long totalRead = 0;
            int read;
            while ((read = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read));
                totalRead += read;
                if (totalBytes > 0)
                {
                    int percent = (int)((totalRead * 100) / totalBytes);
                    onProgress(percent, "Downloading...");
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(sha256Url))
        {
            onProgress(100, "Verifying...");
            try
            {
                string expectedRaw = await Http.GetStringAsync(sha256Url);
                string expected = expectedRaw.Split(' ')[0].Trim().ToLowerInvariant();
                await using FileStream fs = File.OpenRead(tempExe);
                string actual = Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(expected) && expected != actual)
                {
                    File.Delete(tempExe);
                    throw new InvalidOperationException("SHA256 mismatch");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SHA256 verification failed: {ex.Message}");
            }
        }

        return tempExe;
    }

    public void ApplyPreparedUpdate(string preparedExePath)
    {
        string currentExe = Process.GetCurrentProcess().MainModule?.FileName
            ?? Environment.ProcessPath
            ?? Path.Combine(AppContext.BaseDirectory, AssetExeName);

        static string Esc(string? value) => (value ?? "").Replace("'", "''", StringComparison.Ordinal);

        string ps1Path = Path.Combine(Path.GetTempPath(), $"{ProcessName}_swap_{Guid.NewGuid():N}.ps1");
        string logPath = Path.Combine(Path.GetTempPath(), $"{ProcessName}_update_debug.log");

        string command = @"
$ErrorActionPreference = 'Stop'
$log = '{LOG_PATH}'
$oldExe = '{OLD_EXE}'
$newExe = '{NEW_EXE}'
$procName = '{PROC_NAME}'

function Log($msg) {
    ""$(Get-Date -Format 'HH:mm:ss') - $msg"" | Out-File -LiteralPath $log -Append
}

try {
    Log ""Waiting for process to exit...""
    $timeout = 20
    while ($timeout -gt 0) {
        $p = Get-Process -Name $procName -ErrorAction SilentlyContinue
        if (-not $p) { break }
        Start-Sleep -Seconds 1
        $timeout--
    }

    $p = Get-Process -Name $procName -ErrorAction SilentlyContinue
    if ($p) {
        Stop-Process -Name $procName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }

    $retry = 5
    $success = $false
    while ($retry -gt 0) {
        try {
            if (Test-Path -LiteralPath $oldExe) {
                Remove-Item -LiteralPath $oldExe -Force -ErrorAction Stop
            }
            Move-Item -LiteralPath $newExe -Destination $oldExe -Force -ErrorAction Stop
            $success = $true
            break
        } catch {
            $retry--
            Start-Sleep -Seconds 2
        }
    }

    if (-not $success) { throw ""Failed to replace executable after retries."" }
    Start-Process -FilePath $oldExe
}
finally {
    Remove-Item -LiteralPath '{PS1_PATH}' -Force -ErrorAction SilentlyContinue
}
"
        .Replace("{LOG_PATH}", Esc(logPath), StringComparison.Ordinal)
        .Replace("{OLD_EXE}", Esc(currentExe), StringComparison.Ordinal)
        .Replace("{NEW_EXE}", Esc(preparedExePath), StringComparison.Ordinal)
        .Replace("{PROC_NAME}", Esc(ProcessName), StringComparison.Ordinal)
        .Replace("{PS1_PATH}", Esc(ps1Path), StringComparison.Ordinal);

        File.WriteAllText(ps1Path, command, new UTF8Encoding(true));

        Process.Start(new ProcessStartInfo("powershell.exe")
        {
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{ps1Path}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
        });

        Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
    }
}
