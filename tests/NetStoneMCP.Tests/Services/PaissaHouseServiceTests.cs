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
                        id = 339,
                        name = "Mist",
                        open_plots = new List<OpenPlot>
                        {
                            new OpenPlot
                            {
                                district_id = 339,
                                ward_number = 0,
                                plot_number = 0,
                                size = 0,
                                price = 1000,
                                last_updated_time = 1700000000,
                                purchase_system = 2,
                                lotto_entries = 5,
                                lotto_phase = 1
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
            Assert.AreEqual("海霧村 (Mist)", item.District);
            Assert.AreEqual(5, item.LottoEntries);
            Assert.AreEqual(1, item.LottoPhase);
        }

        [Test]
        public async Task GetHouseList_WhenHttpError_ReturnsNull()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var client = new HttpClient(handler);
            var service = new PaissaHouseService(client);

            var result = await service.GetHouseList(999);

            Assert.IsNull(result);
        }
    }
}
