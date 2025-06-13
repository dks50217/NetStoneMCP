using System.Collections.Generic;
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
            Assert.AreEqual(1, System.Linq.Enumerable.Count(result!));
        }
    }
}
