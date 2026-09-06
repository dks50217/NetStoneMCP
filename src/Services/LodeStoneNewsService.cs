using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface ILodeStoneNewsService
    {
        Task<LodeStoneNewsMaintenance?> GetCurrentMaintenances(CancellationToken cancellationToken = default);
        Task<IEnumerable<LodeStoneNewsItem>?> GetTopics(CancellationToken cancellationToken = default);
        Task<LodeStoneNewsItem?> GetPost(string id, CancellationToken cancellationToken = default);
    }

    public class LodeStoneNewsService : ILodeStoneNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LodeStoneNewsService>? _logger;
        private readonly IMemoryCache? _cache;
        private readonly string _endPoint = "https://lodestonenews.com/news";

        private const string MaintenanceCacheKey = "Lodestone_Maintenance";
        private const string TopicsCacheKey = "Lodestone_Topics_jp";
        private const string PostCacheKeyPrefix = "Lodestone_Post_";

        public LodeStoneNewsService(
            HttpClient httpClient, 
            ILogger<LodeStoneNewsService>? logger = null, 
            IMemoryCache? cache = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<LodeStoneNewsMaintenance?> GetCurrentMaintenances(CancellationToken cancellationToken = default)
        {
            if (_cache != null && _cache.TryGetValue(MaintenanceCacheKey, out LodeStoneNewsMaintenance? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var results = await _httpClient.GetFromJsonAsync<LodeStoneNewsMaintenance>(
                    $"{_endPoint}/maintenance/current", 
                    cancellationToken);

                if (results != null)
                {
                    _cache?.Set(MaintenanceCacheKey, results, TimeSpan.FromMinutes(5));
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get current maintenance from LodestoneNews.");
                return null;
            }
        }

        public async Task<IEnumerable<LodeStoneNewsItem>?> GetTopics(CancellationToken cancellationToken = default)
        {
            if (_cache != null && _cache.TryGetValue(TopicsCacheKey, out IEnumerable<LodeStoneNewsItem>? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var results = await _httpClient.GetFromJsonAsync<IEnumerable<LodeStoneNewsItem>>(
                    $"{_endPoint}/topics?locale=jp", 
                    cancellationToken);

                if (results != null)
                {
                    var list = results as IReadOnlyList<LodeStoneNewsItem> ?? new List<LodeStoneNewsItem>(results);
                    _cache?.Set(TopicsCacheKey, (IEnumerable<LodeStoneNewsItem>)list, TimeSpan.FromMinutes(10));
                    return list;
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get topics from LodestoneNews.");
                return null;
            }
        }

        public async Task<LodeStoneNewsItem?> GetPost(string id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{PostCacheKeyPrefix}{id}";
            if (_cache != null && _cache.TryGetValue(cacheKey, out LodeStoneNewsItem? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var results = await _httpClient.GetFromJsonAsync<LodeStoneNewsItem>(
                    $"{_endPoint}/posts/{Uri.EscapeDataString(id)}", 
                    cancellationToken);

                if (results != null)
                {
                    _cache?.Set(cacheKey, results, TimeSpan.FromMinutes(30));
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get post '{PostId}' from LodestoneNews.", id);
                return null;
            }
        }
    }
}
