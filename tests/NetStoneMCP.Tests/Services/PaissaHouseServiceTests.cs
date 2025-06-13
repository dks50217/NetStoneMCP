using System.Collections.Generic;
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
    public class PaissaHouseServiceTests
    {
        [Test]
        public async Task GetHouseList_ReturnsParsedList()
        {
            var model = new PaissaHouseModel
            {
                districts = new List<District>
                {
                    new District
                    {
                        open_plots = new List<OpenPlot>
                        {
                            new OpenPlot
                            {
                                ward_number = 0,
                                plot_number = 0,
                                size = 0,
                                price = 1000,
                                last_updated_time = 1700000000,
                                purchase_system = 2
                            }
                        }
                    }
                }
            };
            var json = JsonSerializer.Serialize(model);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var service = new PaissaHouseService(client);

            var result = await service.GetHouseList(1);

            Assert.IsNotNull(result);
            var item = System.Linq.Enumerable.First(result!);
            Assert.AreEqual("1-1", item.Area);
            Assert.AreEqual("公會", item.Type);
        }
    }
}
