using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using NetStoneMCP.Tests.Helpers;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Services
{
    public class CommonServiceTests
    {
        [Test]
        public async Task GetWorlds_ReturnsWorldList()
        {
            var worlds = new List<WorldDto>
            {
                new WorldDto { id = 1, name = "World1", datacenter_id = 1, datacenter_name = "DC" }
            };
            var json = JsonSerializer.Serialize(worlds);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var service = new CommonService(client);

            var result = await service.GetWorlds();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Count());
            Assert.AreEqual("World1", result!.First().name);
        }
    }
}
