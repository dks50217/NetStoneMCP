using Microsoft.Extensions.Logging;
using NetStone;
using NetStoneMCP.Const;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace NetStoneMCP.Services
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreCategory>?> GetStoreCategories();
        Task<StoreProductDto> GetStoreNewItem(int limit = 5);
        Task<StoreProductDto> GetStoreOnSaleItem(int limit = 5);
        Task<StoreProductDto?> GetStoreProduct(string name, int limit = 5);
    }

    public class StoreService : IStoreService
    {
        private readonly HttpClient _httpClient;

        private readonly ILogger<StoreService> _logger;

        private readonly string _endPoint = "https://api.store.finalfantasyxiv.com/ffxivcatalog/api";

        private int offset = 0;

        private string CurrentCulture = "en-US";

        public StoreService(HttpClient httpClient, ILogger<StoreService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<StoreCategory>?> GetStoreCategories()
        {
            var requestUri = $"{_endPoint}/categories?lang={CurrentCulture}";
            _logger.LogInformation("Calling GetStoreCategories with URI: {RequestUri}", requestUri);

            try
            {
                var result = await _httpClient.GetFromJsonAsync<StoreDto>(requestUri);

                if (result == null)
                {
                    _logger.LogError("GetStoreCategories returned null result.");
                    throw new Exception("GetStoreCategories error.");
                }

                _logger.LogInformation("GetStoreCategories returned {Count} categories.", result.categories?.Count() ?? 0);
                return result.categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in GetStoreCategories.");
                throw;
            }
        }

        public async Task<StoreProductDto> GetStoreNewItem(int limit = 5)
        {
            var requestUri = $"{_endPoint}/products/?lang={CurrentCulture}&currency=USD&limit={limit}&offset={offset}&filters={StoreFilterTypes.NEW}";
            _logger.LogInformation("Calling GetStoreNewItem with URI: {RequestUri}", requestUri);

            try
            {
                var result = await _httpClient.GetFromJsonAsync<StoreProductDto>(requestUri);

                if (result == null)
                {
                    _logger.LogError("GetStoreNewItem returned null result.");
                    throw new Exception("GetStoreNewItem error.");
                }

                _logger.LogInformation("GetStoreNewItem returned {Count} products.", result.products?.Count() ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in GetStoreNewItem.");
                throw;
            }
        }

        public async Task<StoreProductDto> GetStoreOnSaleItem(int limit = 5)
        {
            var requestUri = $"{_endPoint}/products/?lang={CurrentCulture}&currency=USD&limit={limit}&offset={offset}&filters={StoreFilterTypes.ON_SALE}";
            _logger.LogInformation("Calling GetStoreOnSaleItem with URI: {RequestUri}", requestUri);

            try
            {
                var result = await _httpClient.GetFromJsonAsync<StoreProductDto>(requestUri);

                if (result == null)
                {
                    _logger.LogError("GetStoreOnSaleItem returned null result.");
                    throw new Exception("GetStoreOnSaleItem error.");
                }

                _logger.LogInformation("GetStoreOnSaleItem returned {Count} products.", result.products?.Count() ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in GetStoreOnSaleItem.");
                throw;
            }
        }

        public async Task<StoreProductDto?> GetStoreProduct(string name, int limit = 5)
        {
            var categories = await GetStoreCategories();

            var category = categories?.FirstOrDefault(c => c.name?.ToUpper() == name.ToUpper());

            if (category == null)
            {
                return null;
            }

            var requestUri = $"{_endPoint}/products/?lang={CurrentCulture}&currency=USD&limit={limit}&offset={offset}&categoryID={category.id}";

            _logger.LogInformation("Calling GetStoreProduct with URI: {RequestUri}", requestUri);

            var result = await _httpClient.GetFromJsonAsync<StoreProductDto>(requestUri);

            return result;
        }
    }
}
