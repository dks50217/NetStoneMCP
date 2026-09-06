using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using NetStoneMCP.Tests.Helpers;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Services
{
    public class FFXIVCollectServiceTests
    {
        [Test]
        public async Task SearchMountsAsync_ReturnsMountResults()
        {
            var searchResult = new CollectSearchResultDto<CollectItemDto>
            {
                Count = 1,
                Results = new List<CollectItemDto>
                {
                    new()
                    {
                        Id = 171,
                        Name = "Fatter Cat",
                        Patch = "4.45",
                        Seats = 1,
                        Tradeable = false,
                        Sources = new List<CollectSourceDto>
                        {
                            new() { Type = "Premium", Text = "Online Store" }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(searchResult);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = NullLogger<FFXIVCollectService>.Instance;
            var service = new FFXIVCollectService(client, logger);

            var result = await service.SearchMountsAsync("Fatter Cat");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Count());
            var mount = result!.First();
            Assert.AreEqual("Fatter Cat", mount.Name);
            Assert.AreEqual("4.45", mount.Patch);
            Assert.AreEqual(1, mount.Seats);
            Assert.AreEqual("Online Store", mount.Sources.First().Text);
        }

        [Test]
        public async Task SearchMinionsAsync_ReturnsMinionResults()
        {
            var searchResult = new CollectSearchResultDto<CollectItemDto>
            {
                Count = 1,
                Results = new List<CollectItemDto>
                {
                    new()
                    {
                        Id = 375,
                        Name = "The Major-General",
                        Patch = "5.2",
                        Tradeable = false,
                        Sources = new List<CollectSourceDto>
                        {
                            new() { Type = "Achievement", Text = "No More Fish in the Sea I" }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(searchResult);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = NullLogger<FFXIVCollectService>.Instance;
            var service = new FFXIVCollectService(client, logger);

            var result = await service.SearchMinionsAsync("Major-General");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Count());
            Assert.AreEqual("The Major-General", result!.First().Name);
        }

        [Test]
        public async Task SearchAchievementsAsync_ReturnsAchievementResults()
        {
            var searchResult = new CollectSearchResultDto<CollectAchievementDto>
            {
                Count = 1,
                Results = new List<CollectAchievementDto>
                {
                    new()
                    {
                        Id = 1952,
                        Name = "Pal-less Palace III",
                        Points = 20,
                        Patch = "4.0",
                        Reward = new CollectAchievementRewardDto
                        {
                            Type = "Title",
                            Title = new CollectAchievementTitleDto { Name = "The Necromancer" }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(searchResult);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = NullLogger<FFXIVCollectService>.Instance;
            var service = new FFXIVCollectService(client, logger);

            var result = await service.SearchAchievementsAsync("Pal-less Palace");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Count());
            var achievement = result!.First();
            Assert.AreEqual("Pal-less Palace III", achievement.Name);
            Assert.AreEqual(20, achievement.Points);
            Assert.AreEqual("The Necromancer", achievement.Reward?.Title?.Name);
        }

        [Test]
        public async Task SearchCollectablesAsync_NormalizesCategory()
        {
            var searchResult = new CollectSearchResultDto<CollectItemDto>
            {
                Count = 1,
                Results = new List<CollectItemDto>
                {
                    new() { Id = 1, Name = "Test Mount" }
                }
            };

            var json = JsonSerializer.Serialize(searchResult);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = NullLogger<FFXIVCollectService>.Instance;
            var service = new FFXIVCollectService(client, logger);

            var result = await service.SearchCollectablesAsync("mount", "Test");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Count());
        }

        [Test]
        public async Task SearchMountsAsync_WhenHttpError_ReturnsNull()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var client = new HttpClient(handler);
            var logger = NullLogger<FFXIVCollectService>.Instance;
            var service = new FFXIVCollectService(client, logger);

            var result = await service.SearchMountsAsync("Error");

            Assert.IsNull(result);
        }
    }
}
