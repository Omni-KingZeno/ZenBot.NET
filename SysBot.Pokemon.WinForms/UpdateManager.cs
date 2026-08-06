using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SysBot.Base;

namespace SysBot.Pokemon.WinForms;

/// <summary>
/// Checks GitHub for newer ZenBot.NET releases and, when available, downloads and applies the update in place.
/// </summary>
public static class UpdateManager
{
    private const string ApiUrl = "https://api.github.com/repos/Omni-KingZeno/ZenBot.NET/releases/latest";
    private const string ReleasePageUrl = "https://github.com/Omni-KingZeno/ZenBot.NET/releases/latest";
    private const string UserAgent = "ZenBot.NET-UpdateCheck";

    private sealed record GitHubAsset(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("browser_download_url")] string BrowserDownloadUrl);

    private sealed record GitHubRelease(
        [property: JsonPropertyName("tag_name")] string TagName,
        [property: JsonPropertyName("assets")] List<GitHubAsset> Assets);

    public readonly record struct UpdateCheckResult(
        bool IsUpToDate,
        Version? LatestVersion,
        string ReleaseUrl,
        string? AssetDownloadUrl);

    public static async Task<UpdateCheckResult> CheckAsync()
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

        GitHubRelease? release;
        try
        {
            release = await client.GetFromJsonAsync<GitHubRelease>(ApiUrl).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo($"Failed to query GitHub releases API: {ex.Message}", "Update Check");
            return new(true, null, ReleasePageUrl, null);
        }

        if (release is null || string.IsNullOrEmpty(release.TagName))
        {
            LogUtil.LogInfo("Could not read latest release info from GitHub", "Update Check");
            return new(true, null, ReleasePageUrl, null);
        }

        string latestVersionString = release.TagName.TrimStart('v', 'V');

        if (!Version.TryParse(latestVersionString, out Version? latestVersion))
        {
            LogUtil.LogInfo($"Could not parse latest version from tag '{release.TagName}'", "Update Check");
            return new(true, null, ReleasePageUrl, null);
        }

        // Adjust this filter to match your actual asset naming convention
        string? assetUrl = release.Assets
            .FirstOrDefault(a => a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            ?.BrowserDownloadUrl;

        Version currentVersion = ProgramConfig.Version;
        bool isUpToDate = currentVersion >= latestVersion;

        LogUtil.LogInfo(
            $"Latest: {latestVersion} | Current: {currentVersion}",
            isUpToDate ? "Version match" : "Version mismatch");

        return new(isUpToDate, latestVersion, ReleasePageUrl, assetUrl);
    }

    /// <summary>
    /// Downloads the new exe, stages a batch script to replace the running exe once this process exits,
    /// launches it, then exits the current process. Does not return on success.
    /// </summary>
    /// <param name="downloadUrl">Direct URL of the new exe asset.</param>
    /// <param name="progress">
    /// Optional progress reporter. Reports 0-100 when the download's content length is known,
    /// or null to indicate an indeterminate/in-progress state (e.g. length unknown, or staging the installer).
    /// </param>
    public static async Task ApplyUpdateAsync(string downloadUrl, IProgress<int?>? progress = null)
    {
        string currentExePath = Process.GetCurrentProcess().MainModule!.FileName;
        string exeDir = Path.GetDirectoryName(currentExePath)!;
        string tempNewExePath = Path.Combine(Path.GetTempPath(), $"update_{Guid.NewGuid():N}.exe");
        string updaterBatPath = Path.Combine(Path.GetTempPath(), $"update_{Guid.NewGuid():N}.bat");

        // 1. Download the new exe, reporting progress as bytes arrive.
        using (HttpClient client = new())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            await using var httpStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await using var fileStream = File.Create(tempNewExePath);

            var buffer = new byte[81920];
            long bytesRead = 0;
            int read;
            while ((read = await httpStream.ReadAsync(buffer).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                bytesRead += read;

                if (totalBytes is > 0)
                    progress?.Report((int)(bytesRead * 100 / totalBytes.Value));
                else
                    progress?.Report(null); // length unknown, keep it indeterminate
            }
        }

        progress?.Report(100);
        await Task.Delay(250).ConfigureAwait(false);
        progress?.Report(101);

        int currentPid = Environment.ProcessId;

        // 2. Write a batch script that waits for this process to exit,
        //    replaces the old exe with the new one, relaunches it, then deletes itself.
        string batScript = $"""
            @echo off
            :wait
            tasklist /FI "PID eq {currentPid}" | find "{currentPid}" >nul
            if not errorlevel 1 (
                timeout /t 1 /nobreak >nul
                goto wait
            )
            copy /y "{tempNewExePath}" "{currentExePath}"
            del "{tempNewExePath}"
            start "" "{currentExePath}" --updated
            del "%~f0"
            """;

        await File.WriteAllTextAsync(updaterBatPath, batScript).ConfigureAwait(false);

        // 3. Launch the batch script detached, then exit this process so the exe unlocks.
        Process.Start(new ProcessStartInfo
        {
            FileName = updaterBatPath,
            WorkingDirectory = exeDir,
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });

        Environment.Exit(0);
    }
}
