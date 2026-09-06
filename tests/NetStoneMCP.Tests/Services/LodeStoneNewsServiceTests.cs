using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using NetStoneMCP.Tests.Helpers;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Services
{
    public class LodeStoneNewsServiceTests
    {
        [Test]
        public async Task GetCurrentMaintenances_ReturnsResult_AndCaches()
        {
            var maintenance = new LodeStoneNewsMaintenance
            {
                Companion = new List<LodeStoneNewsItem>
                {
                    new() { Title = "Companion App Maintenance", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(2) }
                },
                Game = new List<LodeStoneNewsItem>
                {
                    new() { Title = "All Worlds Maintenance", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(4) }
                }
            };
            var json = JsonSerializer.Serialize(maintenance);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = NullLogger<LodeStoneNewsService>.Instance;
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new LodeStoneNewsService(client, logger, cache);

            var res1 = await service.GetCurrentMaintenances();
            var res2 = await service.GetCurrentMaintenances();

            Assert.IsNotNull(res1);
            Assert.IsNotNull(res1?.Game);
            Assert.AreEqual(1, res1!.Game!.Count);
            Assert.AreEqual("All Worlds Maintenance", res1.Game[0].Title);
            Assert.AreEqual(1, handler.CallCount, "Second call should use cached maintenance data");
        }

        [Test]
        public async Task GetTopics_ReturnsList_AndHandlesNetworkFailureGracefully()
        {
            var topics = new List<LodeStoneNewsItem>
            {
                new() { Id = "topic1", Title = "Valentione's Day Event" }
            };
            var json = JsonSerializer.Serialize(topics);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var service = new LodeStoneNewsService(client);

            var res = await service.GetTopics();

            Assert.IsNotNull(res);
            Assert.AreEqual(1, res!.Count());
            Assert.AreEqual("Valentione's Day Event", res!.First().Title);

            // Error case:
            var errorHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.BadGateway));
            var errorClient = new HttpClient(errorHandler);
            var errorService = new LodeStoneNewsService(errorClient);

            var errorRes = await errorService.GetTopics();
            Assert.IsNull(errorRes, "Should return null on HTTP error without throwing");
        }

        [Test]
        public async Task GetPost_ReturnsPost_AndCaches()
        {
            var post = new LodeStoneNewsItem
            {
                Id = "p123",
                Title = "Patch 7.1 Notes",
                Desc = "Full patch notes details"
            };
            var json = JsonSerializer.Serialize(post);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new LodeStoneNewsService(client, cache: cache);

            var res1 = await service.GetPost("p123");
            var res2 = await service.GetPost("p123");

            Assert.IsNotNull(res1);
            Assert.IsNotNull(res2);
            Assert.AreEqual("Patch 7.1 Notes", res1!.Title);
            Assert.AreEqual(1, handler.CallCount, "Second call for same post should be cached");
        }
    }
}
