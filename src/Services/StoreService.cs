using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Const;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreCategory>?> GetStoreCategories(CancellationToken cancellationToken = default);
        Task<StoreProductDto?> GetStoreNewItem(int limit = 5, CancellationToken cancellationToken = default);
        Task<StoreProductDto?> GetStoreOnSaleItem(int limit = 5, CancellationToken cancellationToken = default);
        Task<StoreProductDto?> GetStoreProduct(string name, int limit = 5, CancellationToken cancellationToken = default);
    }

    public class StoreService : IStoreService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StoreService> _logger;
        private readonly IMemoryCache? _cache;

        private readonly string _endPoint = "https://api.store.finalfantasyxiv.com/ffxivcatalog/api";
        private const string CategoriesCacheKey = "Store_Categories";
        private const int Offset = 0;
        private const string CurrentCulture = "en-US";

        public StoreService(HttpClient httpClient, ILogger<StoreService> logger, IMemoryCache? cache = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IEnumerable<StoreCategory>?> GetStoreCategories(CancellationToken cancellationToken = default)
        {
            if (_cache != null && _cache.TryGetValue(CategoriesCacheKey, out IEnumerable<StoreCategory>? cached) && cached != null)
            {
                return cached;
            }

            var requestUri = $"{_endPoint}/categories?lang={CurrentCulture}";
            _logger.LogInformation("Calling GetStoreCategories with URI: {RequestUri}", requestUri);

            try
            {
                var result = await _httpClient.GetFromJsonAsync<StoreDto>(requestUri, cancellationToken);

                if (result?.categories == null)
                {
                    _logger.LogWarning("GetStoreCategories returned null or empty categories.");
                    return null;
                }

                var categories = result.categories.ToList();
                _cache?.Set(CategoriesCacheKey, (IEnumerable<StoreCategory>)categories, TimeSpan.FromHours(6));

                _logger.LogInformation("GetStoreCategories returned {Count} categories.", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in GetStoreCategories.");
                return null;
            }
        }

        public async Task<StoreProductDto?> GetStoreNewItem(int limit = 5, CancellationToken cancellationToken = default)
        {
            var clampedLimit = Math.Clamp(limit, 1, 50);
            var requestUri = $"{_endPoint}/products/?lang={CurrentCulture}&currency=USD&limit={clampedLimit}&offset={Offset}&filters={StoreFilterTypes.NEW}";
            _logger.LogInformation("Calling GetStoreNewItem with URI: {RequestUri}", requestUri);

            try
            {
                var result = await _httpClient.GetFromJsonAsync<StoreProductDto>(requestUri, cancellationToken);
                if (result == null)
                {
                    _logger.LogWarning("GetStoreNewItem returned null result.");
                    return null;
                }

                _logger.LogInformation("GetStoreNewItem returned {Count} products.", result.products?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in GetStoreNewItem.");
                return null;
            }
        }

        public async Task<StoreProductDto?> GetStoreOnSaleItem(int limit = 5, CancellationToken cancellationToken = default)
        {
            var clampedLimit = Math.Clamp(limit, 1, 50);
            var requestUri = $"{_endPoint}/products/?lang={CurrentCulture}&currency=USD&limit={clampedLimit}&offset={Offset}&filters={StoreFilterTypes.ON_SALE}";
            _logger.LogInformation("Calling GetStoreOnSaleItem with URI: {RequestUri}", requestUri);

            try
            {
                var result = await _httpClient.GetFromJsonAsync<StoreProductDto>(requestUri, cancellationToken);
                if (result == null)
                {
                    _logger.LogWarning("GetStoreOnSaleItem returned null result.");
                    return null;
                }

                _logger.LogInformation("GetStoreOnSaleItem returned {Count} products.", result.products?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in GetStoreOnSaleItem.");
                return null;
            }
        }

        public async Task<StoreProductDto?> GetStoreProduct(string name, int limit = 5, CancellationToken cancellationToken = default)
        {
            var categories = await GetStoreCategories(cancellationToken);
            var category = categories?.FirstOrDefault(c => string.Equals(c.name, name, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                _logger.LogWarning("Store category '{CategoryName}' not found.", name);
                return null;
            }

            var clampedLimit = Math.Clamp(limit, 1, 50);
            var requestUri = $"{_endPoint}/products/?lang={CurrentCulture}&currency=USD&limit={clampedLimit}&offset={Offset}&categoryID={category.id}";
            _logger.LogInformation("Calling GetStoreProduct with URI: {RequestUri}", requestUri);

            try
            {
                var result = await _httpClient.GetFromJsonAsync<StoreProductDto>(requestUri, cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in GetStoreProduct for category: {CategoryName}", name);
                return null;
            }
        }
    }
}
