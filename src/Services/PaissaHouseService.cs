using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Const;
using NetStoneMCP.Dict;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface IPaissaHouseService
    {
        public Task<IEnumerable<PaissaHouseDto>?> GetHouseList(int id, CancellationToken cancellationToken = default);
    }

    public class PaissaHouseService : IPaissaHouseService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaissaHouseService>? _logger;
        private readonly IMemoryCache? _cache;
        private readonly string _endPoint = "https://paissadb.zhu.codes/worlds";

        private const string CacheKeyPrefix = "Paissa_HouseList_";

        public PaissaHouseService(
            HttpClient httpClient, 
            ILogger<PaissaHouseService>? logger = null,
            IMemoryCache? cache = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IEnumerable<PaissaHouseDto>?> GetHouseList(int id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            if (_cache != null && _cache.TryGetValue(cacheKey, out IEnumerable<PaissaHouseDto>? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_endPoint}/{id}", cancellationToken);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                var houseInfo = JsonSerializer.Deserialize<PaissaHouseModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (houseInfo?.districts == null)
                    return null;

                var list = houseInfo.districts
                    .SelectMany(district =>
                        (district.open_plots ?? Enumerable.Empty<OpenPlot>()).Select(plot =>
                        {
                            var wardNumber = plot.ward_number + 1;
                            var plotNumber = plot.plot_number + 1;
                            var districtId = plot.district_id != 0 ? plot.district_id : district.id;
                            var districtName = FFXIVHouseDict.MapDict.GetValueOrDefault(districtId)
                                               ?? district.name
                                               ?? "Unknown";

                            return new PaissaHouseDto
                            {
                                District = districtName,
                                Area = $"{wardNumber}-{plotNumber}",
                                Type = GetPurchaseTypeLabel(plot.purchase_system),
                                Size = FFXIVHouseDict.SizeDict.GetValueOrDefault(plot.size, "Unknown"),
                                Price = plot.price.ToString("N0"),
                                LastUpdateTime = DateTimeOffset.FromUnixTimeSeconds((long)plot.last_updated_time),
                                LottoEntries = plot.lotto_entries,
                                LottoPhase = plot.lotto_phase
                            };
                        }))
                    .ToList();

                _cache?.Set(cacheKey, (IEnumerable<PaissaHouseDto>)list, TimeSpan.FromMinutes(2));

                return list;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching house list for world id: {WorldId}", id);
                return null;
            }
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
