using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetStoneMCP.Services;
using NUnit.Framework;
using Octokit;

namespace NetStoneMCP.Tests.Services
{
    public class CustomServiceTests
    {
        [Test]
        public async Task GetGithubLastReleaseAsync_WhenSuccessful_ReturnsReleaseAndCaches()
        {
            var mockReleasesClient = new Mock<IReleasesClient>();
            var mockRepositoriesClient = new Mock<IRepositoriesClient>();
            var mockGithubClient = new Mock<IGitHubClient>();

            var dummyRelease = new Release(
                url: "https://api.github.com/repos/dks50217/FFXIVChnTextPatch-MC/releases/1",
                htmlUrl: "https://github.com/dks50217/FFXIVChnTextPatch-MC/releases/tag/v1.0.0",
                assetsUrl: "https://api.github.com/repos/dks50217/FFXIVChnTextPatch-MC/releases/1/assets",
                uploadUrl: "https://uploads.github.com/...",
                id: 1,
                nodeId: "MDc6UmVsZWFzZTE=",
                tagName: "v1.0.0",
                targetCommitish: "main",
                name: "Release 1.0.0",
                body: "Release notes changelog",
                draft: false,
                prerelease: false,
                createdAt: DateTimeOffset.UtcNow.AddDays(-1),
                publishedAt: DateTimeOffset.UtcNow,
                author: null,
                tarballUrl: null,
                zipballUrl: null,
                assets: null
            );

            mockReleasesClient
                .Setup(r => r.GetLatest("dks50217", "FFXIVChnTextPatch-MC"))
                .ReturnsAsync(dummyRelease);

            mockRepositoriesClient
                .SetupGet(r => r.Release)
                .Returns(mockReleasesClient.Object);

            mockGithubClient
                .SetupGet(g => g.Repository)
                .Returns(mockRepositoriesClient.Object);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = NullLogger<CustomService>.Instance;
            var service = new CustomService(logger, cache, mockGithubClient.Object);

            var res1 = await service.GetGithubLastReleaseAsync();
            var res2 = await service.GetGithubLastReleaseAsync();

            Assert.IsNotNull(res1);
            Assert.IsNotNull(res2);
            Assert.AreEqual("v1.0.0", res1!.TagName);
            Assert.AreEqual("Release notes changelog", res1.Desc);
            Assert.AreEqual("https://github.com/dks50217/FFXIVChnTextPatch-MC/releases/tag/v1.0.0", res1.HtmlUrl);

            mockReleasesClient.Verify(r => r.GetLatest("dks50217", "FFXIVChnTextPatch-MC"), Times.Once, "Should use cache on second call");
        }

        [Test]
        public async Task GetGithubLastReleaseAsync_WhenApiThrowsException_ReturnsNullGracefully()
        {
            var mockReleasesClient = new Mock<IReleasesClient>();
            var mockRepositoriesClient = new Mock<IRepositoriesClient>();
            var mockGithubClient = new Mock<IGitHubClient>();

            mockReleasesClient
                .Setup(r => r.GetLatest(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ApiException("API Rate Limit Exceeded", System.Net.HttpStatusCode.Forbidden));

            mockRepositoriesClient
                .SetupGet(r => r.Release)
                .Returns(mockReleasesClient.Object);

            mockGithubClient
                .SetupGet(g => g.Repository)
                .Returns(mockRepositoriesClient.Object);

            var logger = NullLogger<CustomService>.Instance;
            var service = new CustomService(logger, null, mockGithubClient.Object);

            var res = await service.GetGithubLastReleaseAsync();

            Assert.IsNull(res, "Should return null on API exception without throwing");
        }
    }
}
