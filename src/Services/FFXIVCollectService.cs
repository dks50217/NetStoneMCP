using Microsoft.Extensions.Logging;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface IFFXIVCollectService
    {
        Task<IEnumerable<CollectItemDto>?> SearchMountsAsync(string name);
        Task<IEnumerable<CollectItemDto>?> SearchMinionsAsync(string name);
        Task<IEnumerable<CollectItemDto>?> SearchEmotesAsync(string name);
        Task<IEnumerable<CollectItemDto>?> SearchHairstylesAsync(string name);
        Task<IEnumerable<CollectItemDto>?> SearchOrchestrionsAsync(string name);
        Task<IEnumerable<CollectItemDto>?> SearchSpellsAsync(string name);
        Task<IEnumerable<CollectItemDto>?> SearchBardingsAsync(string name);
        Task<IEnumerable<CollectItemDto>?> SearchFashionsAsync(string name);
        Task<IEnumerable<CollectAchievementDto>?> SearchAchievementsAsync(string name);
        Task<IEnumerable<CollectItemDto>?> SearchCollectablesAsync(string category, string name);
    }

    public class FFXIVCollectService : IFFXIVCollectService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FFXIVCollectService> _logger;
        private readonly string _baseUrl = "https://ffxivcollect.com/api";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FFXIVCollectService(HttpClient httpClient, ILogger<FFXIVCollectService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private async Task<IEnumerable<T>?> SearchEndpointAsync<T>(string endpoint, string name)
        {
            try
            {
                var queryParam = Uri.EscapeDataString(name);
                var url = $"{_baseUrl}/{endpoint}?name_en_or_name_ja_or_name_fr_or_name_de_cont={queryParam}";

                var response = await _httpClient.GetFromJsonAsync<CollectSearchResultDto<T>>(url, _jsonOptions);
                return response?.Results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search FFXIV Collect endpoint '{Endpoint}' with query '{Query}'", endpoint, name);
                return null;
            }
        }

        public Task<IEnumerable<CollectItemDto>?> SearchMountsAsync(string name)
            => SearchEndpointAsync<CollectItemDto>("mounts", name);

        public Task<IEnumerable<CollectItemDto>?> SearchMinionsAsync(string name)
            => SearchEndpointAsync<CollectItemDto>("minions", name);

        public Task<IEnumerable<CollectItemDto>?> SearchEmotesAsync(string name)
            => SearchEndpointAsync<CollectItemDto>("emotes", name);

        public Task<IEnumerable<CollectItemDto>?> SearchHairstylesAsync(string name)
            => SearchEndpointAsync<CollectItemDto>("hairstyles", name);

        public Task<IEnumerable<CollectItemDto>?> SearchOrchestrionsAsync(string name)
            => SearchEndpointAsync<CollectItemDto>("orchestrions", name);

        public Task<IEnumerable<CollectItemDto>?> SearchSpellsAsync(string name)
            => SearchEndpointAsync<CollectItemDto>("spells", name);

        public Task<IEnumerable<CollectItemDto>?> SearchBardingsAsync(string name)
            => SearchEndpointAsync<CollectItemDto>("bardings", name);

        public Task<IEnumerable<CollectItemDto>?> SearchFashionsAsync(string name)
            => SearchEndpointAsync<CollectItemDto>("fashions", name);

        public Task<IEnumerable<CollectAchievementDto>?> SearchAchievementsAsync(string name)
            => SearchEndpointAsync<CollectAchievementDto>("achievements", name);

        public Task<IEnumerable<CollectItemDto>?> SearchCollectablesAsync(string category, string name)
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

            return SearchEndpointAsync<CollectItemDto>(normalizedCategory, name);
        }
    }
}
