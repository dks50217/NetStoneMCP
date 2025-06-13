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
    public class XIVAPIServiceTests
    {
        [Test]
        public async Task GetItemIdByNameAsync_ReturnsResults()
        {
            var dto = new XivapiSearchResultDto
            {
                Results = new List<XivapiResult>
                {
                    new XivapiResult { RowId = 1, Fields = new XivapiItemFields { Name = "Potion" } }
                }
            };
            var json = JsonSerializer.Serialize(dto);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = new NullLogger<XIVAPIService>();
            var service = new XIVAPIService(client, logger);

            var result = await service.GetItemIdByNameAsync("Potion");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, System.Linq.Enumerable.Count(result!));
            Assert.AreEqual("Potion", System.Linq.Enumerable.First(result!).Fields.Name);
        }
    }
}
