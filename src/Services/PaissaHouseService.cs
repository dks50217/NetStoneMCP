using Microsoft.Extensions.Hosting;
using NetStoneMCP.Const;
using NetStoneMCP.Dict;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface IPaissaHouseService
    {
        public Task<IEnumerable<PaissaHouseDto>?> GetHouseList(int id);
    }

    public class PaissaHouseService : IPaissaHouseService
    {
        private readonly HttpClient _httpClient;

        private readonly string _endPoint = "https://paissadb.zhu.codes/worlds";


        public PaissaHouseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<PaissaHouseDto>?> GetHouseList(int id)
        {
            var response = await _httpClient.GetAsync($"{_endPoint}/{id}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var houseInfo = JsonSerializer.Deserialize<PaissaHouseModel>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (houseInfo?.districts == null)
                return null;

            var list = houseInfo.districts
                .SelectMany(district =>
                    district.open_plots.Select(plot =>
                    {
                        plot.ward_number++;
                        plot.plot_number++;

                        return new PaissaHouseDto
                        {
                            Area = $"{plot.ward_number}-{plot.plot_number}",
                            Type = GetPurchaseTypeLabel(plot.purchase_system),
                            Size = FFXIVHouseDict.SizeDict.GetValueOrDefault(plot.size, "Unknown"),
                            Price = plot.price.ToString("N0"),
                            LastUpdateTime = DateTimeOffset.FromUnixTimeSeconds((long)plot.last_updated_time).DateTime
                        };
                    }))
                .ToList();

            return list;
        }

        private string GetPurchaseTypeLabel(int e)
        {
            if ((e & (PurchaseHouseTypes.FREE_COMPANY | PurchaseHouseTypes.INDIVIDUAL)) == (PurchaseHouseTypes.FREE_COMPANY | PurchaseHouseTypes.INDIVIDUAL))
            {
                return "公會/個人";
            }
            else if ((e & PurchaseHouseTypes.FREE_COMPANY) != 0)
            {
                return "公會";
            }
            else
            {
                return "個人";
            }
        }
    }
}
