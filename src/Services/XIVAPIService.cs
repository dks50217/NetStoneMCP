using Microsoft.Extensions.Logging;
using NetStone.StaticData;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface IXIVAPIService
    {
        public Task<IEnumerable<XivapiResult>?> GetItemIdByNameAsync(string itemName);
        public Task<IEnumerable<XivapiRecipeResultDto>?> GetRecipesByItemIdAsync(int itemId);
    }

    public class XIVAPIService : IXIVAPIService
    {
        private readonly HttpClient _httpClient;

        private readonly ILogger<XIVAPIService> _logger;

        private readonly string _endPoint = "https://v2.xivapi.com/api";
        private readonly string _endPointOld = "https://xivapi.com";

        public XIVAPIService(HttpClient httpClient, ILogger<XIVAPIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<XivapiResult>?> GetItemIdByNameAsync(string itemName)
        {
            var query = $"\"{itemName}\"";

            var url = $"{_endPoint}/search?query=Name~{Uri.EscapeDataString(query)}&sheets=Item&fields=Name";
            
            var response = await _httpClient.GetFromJsonAsync<XivapiSearchResultDto>(url);

            if (response == null)
            {
                return null;
            }

            return response.Results;
        }

        public async Task<IEnumerable<XivapiRecipeResultDto>?> GetRecipesByItemIdAsync(int itemId)
        {
            var sheets = "Recipe";

            var url = $"{_endPoint}/search?query=Crafted{itemId}&sheets={sheets}&fields=ItemResultTargetID,Name,ClassJobLevel,Ingredients";

            var response = await _httpClient.GetFromJsonAsync<IEnumerable<XivapiRecipeResultDto>>(url);

            return response;
        }
    }
}
