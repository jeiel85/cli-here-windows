using System.Diagnostics;
using System.IO.Compression;
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
    private const string AssetZipName = "CliHere-win-x64.zip";
    private const string ProcessName = "CliHere";
    private const string UserAgent = "CliHere-Updater";

    private const string ApiListUrl = $"https://api.github.com/repos/{Repo}/releases?per_page=30";
    public const string ReleasePage = $"https://github.com/{Repo}/releases/latest";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };

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

        string? zipUrl = null;
        string? shaUrl = null;
        foreach (JsonElement asset in latest.element.GetProperty("assets").EnumerateArray())
        {
            string name = asset.GetProperty("name").GetString() ?? "";
            string url = asset.GetProperty("browser_download_url").GetString() ?? "";
            if (name.Equals(AssetZipName, StringComparison.OrdinalIgnoreCase)) zipUrl = url;
            if (name.Equals($"{AssetZipName}.sha256", StringComparison.OrdinalIgnoreCase)) shaUrl = url;
        }

        if (zipUrl is null) return null;
        return new UpdateInfo(latest.version, zipUrl, shaUrl ?? "", notesBuilder.ToString().TrimEnd());
    }

    public async Task<string> DownloadAndPrepareUpdateAsync(string downloadUrl, string sha256Url, Action<int, string> onProgress)
    {
        string tempZip = Path.Combine(Path.GetTempPath(), $"{ProcessName}_new_{Guid.NewGuid():N}.zip");
        onProgress(0, "Downloading...");

        try
        {
            using (HttpResponseMessage response = await Http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                long totalBytes = response.Content.Headers.ContentLength ?? 0;

                using Stream contentStream = await response.Content.ReadAsStreamAsync();
                await using FileStream fileStream = new(tempZip, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

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
        }
        catch (HttpRequestException ex)
        {
            throw new UpdateCheckException($"Network error during download: {ex.Message}", UpdateCheckErrorKind.Network, inner: ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new UpdateCheckException($"Download timed out: {ex.Message}", UpdateCheckErrorKind.Timeout, inner: ex);
        }

        if (!string.IsNullOrWhiteSpace(sha256Url))
        {
            onProgress(100, "Verifying...");
            try
            {
                string expectedRaw = await Http.GetStringAsync(sha256Url);
                string expected = expectedRaw.Split(' ')[0].Trim().ToLowerInvariant();
                await using FileStream fs = File.OpenRead(tempZip);
                string actual = Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(expected) && expected != actual)
                {
                    File.Delete(tempZip);
                    throw new InvalidOperationException("SHA256 mismatch: File integrity check failed. Please download again.");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"SHA256 download failed (non-fatal): {ex.Message}");
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                Debug.WriteLine($"SHA256 verification failed (non-fatal): {ex.Message}");
            }
        }

        string extractDir = Path.Combine(Path.GetTempPath(), $"{ProcessName}_extract_{Guid.NewGuid():N}");
        Directory.CreateDirectory(extractDir);
        ZipFile.ExtractToDirectory(tempZip, extractDir, overwriteFiles: true);
        try { File.Delete(tempZip); } catch { /* best-effort */ }

        return extractDir;
    }

    public void ApplyPreparedUpdate(string preparedExtractDir)
    {
        string currentExe = Process.GetCurrentProcess().MainModule?.FileName
            ?? Environment.ProcessPath
            ?? Path.Combine(AppContext.BaseDirectory, $"{ProcessName}.exe");
        string installDir = Path.GetDirectoryName(currentExe)
            ?? throw new InvalidOperationException("Cannot resolve install directory.");

        static string Esc(string? value) => (value ?? "").Replace("'", "''", StringComparison.Ordinal);

        string ps1Path = Path.Combine(Path.GetTempPath(), $"{ProcessName}_swap_{Guid.NewGuid():N}.ps1");
        string logPath = Path.Combine(Path.GetTempPath(), $"{ProcessName}_update_debug.log");

        string command = @"
$ErrorActionPreference = 'Stop'
$log = '{LOG_PATH}'
$installDir = '{INSTALL_DIR}'
$extractDir = '{EXTRACT_DIR}'
$exeName = '{EXE_NAME}'
$procName = '{PROC_NAME}'
$ps1Self = '{PS1_PATH}'

function Log($msg) {
    try {
        ""$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $msg"" | Out-File -LiteralPath $log -Append -Encoding utf8
    } catch {}
}

try {
    Log ""=== Update started ===""
    Log ""installDir=$installDir""
    Log ""extractDir=$extractDir""

    Log ""Waiting for $procName to exit (up to 20s)...""
    $timeout = 20
    while ($timeout -gt 0) {
        $p = Get-Process -Name $procName -ErrorAction SilentlyContinue
        if (-not $p) { break }
        Start-Sleep -Seconds 1
        $timeout--
    }

    $p = Get-Process -Name $procName -ErrorAction SilentlyContinue
    if ($p) {
        Log ""Process still running, force-killing $($p.Count) instance(s)""
        Stop-Process -Name $procName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }
    Log ""Process exited""

    if (-not (Test-Path -LiteralPath $extractDir)) { throw ""Extract dir not found: $extractDir"" }
    if (-not (Test-Path -LiteralPath $installDir)) { throw ""Install dir not found: $installDir"" }

    $files = Get-ChildItem -LiteralPath $extractDir -Recurse -File
    Log ""Copying $($files.Count) file(s) into install dir""

    $retry = 5
    $copied = $false
    while ($retry -gt 0) {
        try {
            foreach ($f in $files) {
                $rel = $f.FullName.Substring($extractDir.Length).TrimStart('\','/')
                $dest = Join-Path $installDir $rel
                $destDir = Split-Path $dest -Parent
                if ($destDir -and -not (Test-Path -LiteralPath $destDir)) {
                    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
                }
                Copy-Item -LiteralPath $f.FullName -Destination $dest -Force -ErrorAction Stop
            }
            $copied = $true
            break
        } catch {
            $errMsg = $_.Exception.Message
            Log ""Copy attempt failed: $errMsg""
            if ($errMsg -match 'being used by another process') {
                Log ""File locked - another app may be using CliHere""
            } elseif ($errMsg -match 'Access to the path.*is denied') {
                Log ""Permission denied - may need admin rights""
            }
            $retry--
            Start-Sleep -Seconds 2
        }
    }

    if (-not $copied) { throw ""Failed to copy update files after 5 retries. The app may need to be reinstalled manually."" }
    Log ""Files copied successfully""

    $exePath = Join-Path $installDir $exeName
    Log ""Starting $exePath""
    Start-Process -FilePath $exePath
    Log ""=== Update completed ===""
}
catch {
    Log ""ERROR: $($_.Exception.Message)""
    Log $_.ScriptStackTrace
}
finally {
    try { Remove-Item -LiteralPath $extractDir -Recurse -Force -ErrorAction SilentlyContinue } catch {}
    try { Remove-Item -LiteralPath $ps1Self -Force -ErrorAction SilentlyContinue } catch {}
}
"
        .Replace("{LOG_PATH}", Esc(logPath), StringComparison.Ordinal)
        .Replace("{INSTALL_DIR}", Esc(installDir), StringComparison.Ordinal)
        .Replace("{EXTRACT_DIR}", Esc(preparedExtractDir), StringComparison.Ordinal)
        .Replace("{EXE_NAME}", Esc($"{ProcessName}.exe"), StringComparison.Ordinal)
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
