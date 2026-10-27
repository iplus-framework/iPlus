// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using gip.tool.installerAndUpdater.Models;

namespace gip.tool.installerAndUpdater.Services
{
    /// <summary>
    /// One downloadable asset of a GitHub release.
    /// </summary>
    public sealed class GitHubAsset
    {
        public string Name { get; set; } = string.Empty;

        public string DownloadUrl { get; set; } = string.Empty;

        public long Size { get; set; }
    }

    /// <summary>
    /// A GitHub release with its assets.
    /// </summary>
    public sealed class GitHubRelease
    {
        public string TagName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool Prerelease { get; set; }

        public DateTime PublishedAt { get; set; }

        public List<GitHubAsset> Assets { get; set; } = new List<GitHubAsset>();

        /// <summary>
        /// Version without the leading "v".
        /// </summary>
        public string Version => TagName.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            ? TagName.Substring(1)
            : TagName;
    }

    /// <summary>
    /// Reads releases from the public GitHub REST API and downloads assets.
    /// </summary>
    /// <remarks>
    /// No authentication is used: the repositories are public and the anonymous
    /// rate limit (60 requests/hour per IP) is far above what an installer needs.
    /// </remarks>
    public sealed class GitHubReleaseService : IDisposable
    {
        private const string ApiBase = "https://api.github.com";

        private readonly HttpClient _http;

        public GitHubReleaseService()
        {
            _http = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            _http.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("iPlus-Installer", "1.0"));
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        }

        /// <summary>
        /// Lists the releases of a repository, newest first.
        /// </summary>
        public async Task<List<GitHubRelease>> GetReleasesAsync(
            string repository,
            bool includePrereleases,
            CancellationToken cancellationToken)
        {
            string url = ApiBase + "/repos/" + repository + "/releases?per_page=30";
            using HttpResponseMessage response = await _http.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            List<GitHubRelease> releases = new List<GitHubRelease>();
            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                GitHubRelease release = new GitHubRelease
                {
                    TagName = element.TryGetProperty("tag_name", out JsonElement tag) ? tag.GetString() ?? string.Empty : string.Empty,
                    Name = element.TryGetProperty("name", out JsonElement name) ? name.GetString() ?? string.Empty : string.Empty,
                    Prerelease = element.TryGetProperty("prerelease", out JsonElement pre) && pre.GetBoolean()
                };

                if (element.TryGetProperty("published_at", out JsonElement published)
                    && DateTime.TryParse(published.GetString(), out DateTime publishedAt))
                {
                    release.PublishedAt = publishedAt;
                }

                if (element.TryGetProperty("assets", out JsonElement assets))
                {
                    foreach (JsonElement asset in assets.EnumerateArray())
                    {
                        release.Assets.Add(new GitHubAsset
                        {
                            Name = asset.TryGetProperty("name", out JsonElement assetName) ? assetName.GetString() ?? string.Empty : string.Empty,
                            DownloadUrl = asset.TryGetProperty("browser_download_url", out JsonElement downloadUrl) ? downloadUrl.GetString() ?? string.Empty : string.Empty,
                            Size = asset.TryGetProperty("size", out JsonElement size) && size.TryGetInt64(out long bytes) ? bytes : 0
                        });
                    }
                }

                if (includePrereleases || !release.Prerelease)
                    releases.Add(release);
            }

            return releases;
        }

        /// <summary>
        /// Returns the newest release, optionally restricted to a tag.
        /// </summary>
        public async Task<GitHubRelease?> GetReleaseAsync(
            string repository,
            string tag,
            bool includePrereleases,
            CancellationToken cancellationToken)
        {
            List<GitHubRelease> releases = await GetReleasesAsync(repository, includePrereleases, cancellationToken);
            if (releases.Count == 0)
                return null;

            if (string.IsNullOrWhiteSpace(tag))
                return releases[0];

            string normalized = tag.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? tag : "v" + tag;
            return releases.FirstOrDefault(r =>
                       string.Equals(r.TagName, tag, StringComparison.OrdinalIgnoreCase)
                       || string.Equals(r.TagName, normalized, StringComparison.OrdinalIgnoreCase))
                   ?? releases[0];
        }

        /// <summary>
        /// Picks the asset that matches the current platform and product.
        /// </summary>
        /// <remarks>
        /// Preference order:
        /// 1. the combined <c>all.zip</c> (contains every platform),
        /// 2. the platform-specific zip for this OS/architecture.
        /// </remarks>
        public static GitHubAsset? SelectAsset(GitHubRelease release, string productPrefix, string platformFolder)
        {
            List<GitHubAsset> zips = release.Assets
                .Where(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                .Where(a => a.Name.StartsWith(productPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            GitHubAsset? combined = zips.FirstOrDefault(a =>
                a.Name.Contains("-all", StringComparison.OrdinalIgnoreCase));
            if (combined != null)
                return combined;

            return zips.FirstOrDefault(a =>
                a.Name.Contains(platformFolder, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Downloads an asset to <paramref name="targetPath"/>.
        /// </summary>
        public async Task DownloadAsync(
            GitHubAsset asset,
            string targetPath,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await _http.GetAsync(
                asset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long total = response.Content.Headers.ContentLength ?? asset.Size;

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(targetPath)) ?? ".");

            using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken);
            using FileStream target = new FileStream(
                targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

            byte[] buffer = new byte[81920];
            long received = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                received += read;

                if (total > 0)
                {
                    progress?.Report(new ProgressReport(
                        $"Downloading {asset.Name} ({received / 1024 / 1024} of {total / 1024 / 1024} MB)",
                        (double)received / total));
                }
            }
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
