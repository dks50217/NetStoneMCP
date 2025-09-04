using NetStoneMCP.Model;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
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
        Task<GithubReleaseDto?> GetGithubLastReleaseAsync();
    }

    public class CustomService : ICustomService
    {
        public async Task<GithubReleaseDto?> GetGithubLastReleaseAsync()
        {
            string targetUsername = "dks50217";
            string targetRepositoryName = "FFXIVChnTextPatch-MC";

            var github = new GitHubClient(new ProductHeaderValue("GitHubUpdateChecker"));

            try
            {
                var releases = await github.Repository.Release.GetLatest(targetUsername, targetRepositoryName);

                if (releases == null)
                {
                    return null;
                }

                return new GithubReleaseDto
                {
                    HtmlUrl = releases.HtmlUrl,
                    TagName = releases.TagName
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
