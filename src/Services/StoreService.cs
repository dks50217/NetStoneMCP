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
    }

    public class StoreService : IStoreService
    {
        private readonly HttpClient _httpClient;

        private readonly string _endPoint = "https://api.store.finalfantasyxiv.com/ffxivcatalog/api";

        private int offset = 0;

        public StoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<StoreCategory>?> GetStoreCategories()
        {
            var requestUri = $"{_endPoint}/categories?lang={CultureInfo.CurrentCulture.Name}";

            var result = await _httpClient.GetFromJsonAsync<StoreDto>(requestUri);

            if (result == null)
                throw new Exception("GetStoreCategories error.");

            return result.categories;
        }

        public async Task<StoreProductDto> GetStoreNewItem(int limit = 5)
        {
            var requestUri = $"{_endPoint}/products/?lang={CultureInfo.CurrentCulture.Name}&currency=USD&limit={limit}&offset={offset}&filters={StoreFilterTypes.NEW}";

            var result = await _httpClient.GetFromJsonAsync<StoreProductDto>(requestUri);

            if (result == null)
                throw new Exception("GetStoreNewItem error.");

            return result;
        }

        public async Task<StoreProductDto> GetStoreOnSaleItem(int limit = 5)
        {
            var requestUri = $"{_endPoint}/products/?lang={CultureInfo.CurrentCulture.Name}&currency=USD&limit={limit}&offset={offset}&filters={StoreFilterTypes.ON_SALE}";

            var result = await _httpClient.GetFromJsonAsync<StoreProductDto>(requestUri);

            if (result == null)
                throw new Exception("GetStoreOnSaleItem error.");

            return result;
        }

        //TODO Category, SubCategory
    }
}
