using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NetStoneMCP.Services;
using NetStoneMCP.Tests.Helpers;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Services
{
    public class ThaliakServiceTests
    {
        [Test]
        public async Task GetLatestVersionsAsync_ReturnsMappedVersionsWithFriendlyNames()
        {
            var graphQlResponse = @"{
  ""data"": {
    ""r0"": { ""slug"": ""2b5cbc63"", ""latestVersion"": { ""versionString"": ""2026.07.13.0000.0001"" } },
    ""r1"": { ""slug"": ""4e9a232b"", ""latestVersion"": { ""versionString"": ""2026.08.11.0000.0000"" } },
    ""r6"": { ""slug"": ""6cfeab11"", ""latestVersion"": { ""versionString"": ""2026.08.11.0000.0000"" } }
  }
}";

            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(graphQlResponse, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = NullLogger<ThaliakService>.Instance;
            var service = new ThaliakService(client, logger);

            var result = await service.GetLatestVersionsAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);

            Assert.AreEqual("2b5cbc63", result[0].Slug);
            Assert.AreEqual("Boot", result[0].Name);
            Assert.AreEqual("2026.07.13.0000.0001", result[0].VersionString);

            Assert.AreEqual("4e9a232b", result[1].Slug);
            Assert.AreEqual("Base Game", result[1].Name);

            Assert.AreEqual("6cfeab11", result[2].Slug);
            Assert.AreEqual("ex5 (Dawntrail)", result[2].Name);
        }

        [Test]
        public async Task GetLatestVersionsAsync_WhenHttpError_ReturnsEmptyList()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var client = new HttpClient(handler);
            var logger = NullLogger<ThaliakService>.Instance;
            var service = new ThaliakService(client, logger);

            var result = await service.GetLatestVersionsAsync();

            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }
    }
}
