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
    public class StoreServiceTests
    {
        [Test]
        public async Task GetStoreCategories_ReturnsList()
        {
            var dto = new StoreDto
            {
                categories = new List<StoreCategory>
                {
                    new StoreCategory{ id = 1, name = "Cat" }
                }
            };
            var json = JsonSerializer.Serialize(dto);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = new NullLogger<StoreService>();
            var service = new StoreService(client, logger);

            var result = await service.GetStoreCategories();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Count());
        }

        [Test]
        public async Task GetStoreCategories_WithCache_UsesCacheOnSecondCall()
        {
            var dto = new StoreDto
            {
                categories = new List<StoreCategory>
                {
                    new StoreCategory{ id = 1, name = "Mounts" }
                }
            };
            var json = JsonSerializer.Serialize(dto);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = new NullLogger<StoreService>();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new StoreService(client, logger, cache);

            var result1 = await service.GetStoreCategories();
            var result2 = await service.GetStoreCategories();

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(1, handler.CallCount, "Should use memory cache and avoid second HTTP request");
        }

        [Test]
        public async Task GetStoreProduct_WhenCategoryNotFound_ReturnsNullWithoutCallingProductEndpoint()
        {
            var dto = new StoreDto
            {
                categories = new List<StoreCategory>
                {
                    new StoreCategory{ id = 1, name = "Costumes" }
                }
            };
            var json = JsonSerializer.Serialize(dto);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = new NullLogger<StoreService>();
            var service = new StoreService(client, logger);

            var result = await service.GetStoreProduct("NonExistentCategory");

            Assert.IsNull(result);
            Assert.AreEqual(1, handler.CallCount, "Should only have called categories endpoint");
        }

        [Test]
        public async Task GetStoreNewItem_OnHttpError_ReturnsNullGracefully()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var client = new HttpClient(handler);
            var logger = new NullLogger<StoreService>();
            var service = new StoreService(client, logger);

            var result = await service.GetStoreNewItem();

            Assert.IsNull(result);
        }
    }
}
