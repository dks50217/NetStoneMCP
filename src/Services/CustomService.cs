using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Model;
using Octokit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface ICustomService
    {
        /// <summary>
        /// Gets the most recent release FFXIV ChnTextPatch from GitHub.
        /// </summary>
        /// <returns>
        /// A <see cref="GithubReleaseDto"/> object representing the latest release, 
        /// or <c>null</c> if no release is found.
        /// </returns>
        Task<GithubReleaseDto?> GetGithubLastReleaseAsync(CancellationToken cancellationToken = default);
    }

    public class CustomService : ICustomService
    {
        private readonly ILogger<CustomService>? _logger;
        private readonly IMemoryCache? _cache;
        private readonly IGitHubClient _githubClient;
        private const string CacheKey = "GitHub_LastRelease_FFXIVChnTextPatch";

        public CustomService(
            ILogger<CustomService>? logger = null, 
            IMemoryCache? cache = null, 
            IGitHubClient? githubClient = null)
        {
            _logger = logger;
            _cache = cache;
            _githubClient = githubClient ?? new GitHubClient(new ProductHeaderValue("GitHubUpdateChecker"));
        }

        public async Task<GithubReleaseDto?> GetGithubLastReleaseAsync(CancellationToken cancellationToken = default)
        {
            if (_cache != null && _cache.TryGetValue(CacheKey, out GithubReleaseDto? cached) && cached != null)
            {
                return cached;
            }

            string targetUsername = "dks50217";
            string targetRepositoryName = "FFXIVChnTextPatch-MC";

            try
            {
                var releases = await _githubClient.Repository.Release.GetLatest(targetUsername, targetRepositoryName);

                if (releases == null)
                {
                    return null;
                }

                var dto = new GithubReleaseDto
                {
                    HtmlUrl = releases.HtmlUrl,
                    TagName = releases.TagName,
                    Desc = releases.Body,
                    CreatedAt = releases.CreatedAt,
                    PublishedAt = releases.PublishedAt,
                };

                _cache?.Set(CacheKey, dto, TimeSpan.FromMinutes(15));
                return dto;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to fetch latest GitHub release for {Owner}/{Repo}", targetUsername, targetRepositoryName);
                return null;
            }
        }
    }
}
