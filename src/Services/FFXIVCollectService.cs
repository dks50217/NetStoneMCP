using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface IFFXIVCollectService
    {
        Task<IEnumerable<CollectItemDto>?> SearchMountsAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<CollectItemDto>?> SearchMinionsAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<CollectItemDto>?> SearchEmotesAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<CollectItemDto>?> SearchHairstylesAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<CollectItemDto>?> SearchOrchestrionsAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<CollectItemDto>?> SearchSpellsAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<CollectItemDto>?> SearchBardingsAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<CollectItemDto>?> SearchFashionsAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<CollectAchievementDto>?> SearchAchievementsAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<CollectItemDto>?> SearchCollectablesAsync(string category, string name, CancellationToken cancellationToken = default);
    }

    public class FFXIVCollectService : IFFXIVCollectService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FFXIVCollectService> _logger;
        private readonly IMemoryCache? _cache;
        private readonly string _baseUrl = "https://ffxivcollect.com/api";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FFXIVCollectService(HttpClient httpClient, ILogger<FFXIVCollectService> logger, IMemoryCache? cache = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        private async Task<IEnumerable<T>?> SearchEndpointAsync<T>(string endpoint, string name, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"Collect_{endpoint}_{name.Trim().ToLowerInvariant()}";
            if (_cache != null && _cache.TryGetValue(cacheKey, out IEnumerable<T>? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var queryParam = Uri.EscapeDataString(name);
                var url = $"{_baseUrl}/{endpoint}?name_en_or_name_ja_or_name_fr_or_name_de_cont={queryParam}";

                var response = await _httpClient.GetFromJsonAsync<CollectSearchResultDto<T>>(url, _jsonOptions, cancellationToken);
                var results = response?.Results;

                if (results != null)
                {
                    var list = results.ToList();
                    _cache?.Set(cacheKey, (IEnumerable<T>)list, TimeSpan.FromMinutes(30));
                    return list;
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search FFXIV Collect endpoint '{Endpoint}' with query '{Query}'", endpoint, name);
                return null;
            }
        }

        public Task<IEnumerable<CollectItemDto>?> SearchMountsAsync(string name, CancellationToken cancellationToken = default)
            => SearchEndpointAsync<CollectItemDto>("mounts", name, cancellationToken);

        public Task<IEnumerable<CollectItemDto>?> SearchMinionsAsync(string name, CancellationToken cancellationToken = default)
            => SearchEndpointAsync<CollectItemDto>("minions", name, cancellationToken);

        public Task<IEnumerable<CollectItemDto>?> SearchEmotesAsync(string name, CancellationToken cancellationToken = default)
            => SearchEndpointAsync<CollectItemDto>("emotes", name, cancellationToken);

        public Task<IEnumerable<CollectItemDto>?> SearchHairstylesAsync(string name, CancellationToken cancellationToken = default)
            => SearchEndpointAsync<CollectItemDto>("hairstyles", name, cancellationToken);

        public Task<IEnumerable<CollectItemDto>?> SearchOrchestrionsAsync(string name, CancellationToken cancellationToken = default)
            => SearchEndpointAsync<CollectItemDto>("orchestrions", name, cancellationToken);

        public Task<IEnumerable<CollectItemDto>?> SearchSpellsAsync(string name, CancellationToken cancellationToken = default)
            => SearchEndpointAsync<CollectItemDto>("spells", name, cancellationToken);

        public Task<IEnumerable<CollectItemDto>?> SearchBardingsAsync(string name, CancellationToken cancellationToken = default)
            => SearchEndpointAsync<CollectItemDto>("bardings", name, cancellationToken);

        public Task<IEnumerable<CollectItemDto>?> SearchFashionsAsync(string name, CancellationToken cancellationToken = default)
            => SearchEndpointAsync<CollectItemDto>("fashions", name, cancellationToken);

        public Task<IEnumerable<CollectAchievementDto>?> SearchAchievementsAsync(string name, CancellationToken cancellationToken = default)
            => SearchEndpointAsync<CollectAchievementDto>("achievements", name, cancellationToken);

        public Task<IEnumerable<CollectItemDto>?> SearchCollectablesAsync(string category, string name, CancellationToken cancellationToken = default)
        {
            var normalizedCategory = category.ToLowerInvariant().Trim();

            // Map common aliases or singular to plural endpoint names
            normalizedCategory = normalizedCategory switch
            {
                "mount" => "mounts",
                "minion" => "minions",
                "emote" => "emotes",
                "hairstyle" => "hairstyles",
                "orchestrion" => "orchestrions",
                "spell" => "spells",
                "barding" => "bardings",
                "fashion" or "accessory" or "accessories" => "fashions",
                _ => normalizedCategory
            };

            return SearchEndpointAsync<CollectItemDto>(normalizedCategory, name, cancellationToken);
        }
    }
}
