using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetStone.StaticData;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface IXIVAPIService
    {
        public Task<IEnumerable<XivapiResult>?> GetItemIdByNameAsync(string itemName, CancellationToken cancellationToken = default);
        public Task<IEnumerable<XivapiRecipeResultDto>?> GetRecipesByItemIdAsync(int itemId, CancellationToken cancellationToken = default);
    }

    public class XIVAPIService : IXIVAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<XIVAPIService> _logger;
        private readonly IMemoryCache? _cache;

        private readonly string _endPoint = "https://v2.xivapi.com/api";
        private const string ItemCacheKeyPrefix = "Xivapi_Item_";
        private const string RecipeCacheKeyPrefix = "Xivapi_Recipe_";

        public XIVAPIService(HttpClient httpClient, ILogger<XIVAPIService> logger, IMemoryCache? cache = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IEnumerable<XivapiResult>?> GetItemIdByNameAsync(string itemName, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{ItemCacheKeyPrefix}{itemName.Trim().ToLowerInvariant()}";
            if (_cache != null && _cache.TryGetValue(cacheKey, out IEnumerable<XivapiResult>? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var query = $"\"{itemName}\"";
                var url = $"{_endPoint}/search?query=Name~{Uri.EscapeDataString(query)}&sheets=Item&fields=Name";

                var response = await _httpClient.GetFromJsonAsync<XivapiSearchResultDto>(url, cancellationToken);
                var results = response?.Results;

                if (results != null)
                {
                    var list = results.ToList();
                    _cache?.Set(cacheKey, (IEnumerable<XivapiResult>)list, TimeSpan.FromHours(1));
                    return list;
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching item by name: {ItemName}", itemName);
                return null;
            }
        }

        public async Task<IEnumerable<XivapiRecipeResultDto>?> GetRecipesByItemIdAsync(int itemId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{RecipeCacheKeyPrefix}{itemId}";
            if (_cache != null && _cache.TryGetValue(cacheKey, out IEnumerable<XivapiRecipeResultDto>? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var sheets = "Recipe";
                var fields = "ItemResult,AmountResult,RecipeLevelTable,AmountIngredient,Ingredient";
                var url = $"{_endPoint}/search?query=ItemResult={itemId}&sheets={sheets}&fields={fields}";

                var response = await _httpClient.GetFromJsonAsync<XivapiRecipeSearchResponseDto>(url, cancellationToken);

                if (response?.Results is null)
                {
                    return null;
                }

                var list = new List<XivapiRecipeResultDto>();

                foreach (var entry in response.Results)
                {
                    var f = entry.Fields;
                    var recipeDto = new XivapiRecipeResultDto
                    {
                        ItemResultTargetID = f.ItemResult?.Value ?? itemId,
                        Name = f.ItemResult?.Fields?.Name ?? string.Empty,
                        ClassJobLevel = f.RecipeLevelTable?.Fields?.ClassJobLevel ?? 0,
                        Ingredients = new List<RecipeIngredient>()
                    };

                    for (int i = 0; i < f.Ingredient.Count; i++)
                    {
                        var ing = f.Ingredient[i];
                        var amount = i < f.AmountIngredient.Count ? f.AmountIngredient[i] : 0;

                        if (ing.Value > 0 && amount > 0)
                        {
                            recipeDto.Ingredients.Add(new RecipeIngredient
                            {
                                ID = ing.Value,
                                Name = ing.Fields?.Name,
                                Amount = amount
                            });
                        }
                    }

                    list.Add(recipeDto);
                }

                _cache?.Set(cacheKey, (IEnumerable<XivapiRecipeResultDto>)list, TimeSpan.FromHours(1));
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching recipes for item id: {ItemId}", itemId);
                return null;
            }
        }
    }
}
