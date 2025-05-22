using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.ServerSentEvents;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class StoreTool(IStoreService storeService, ILogger<StoreTool> logger)
    {
        private readonly IStoreService _storeService = storeService;
        private readonly ILogger<StoreTool> _logger = logger;

        [McpServerTool(Name = "get_store_categories", Title = "Get FFXIV store categories")]
        [Description("Get FFXIV store categories")]
        public async Task<IEnumerable<StoreCategory>?> GetStoreCategories()
        {
            return await _storeService.GetStoreCategories();
        }

        [McpServerTool(Name = "get_store_new_product", Title = "Get FFXIV store new product")]
        [Description("Get FFXIV store new product")]
        public async Task<StoreProductDto> GetStoreNewItem()
        {
            return await _storeService.GetStoreNewItem();
        }

        [McpServerTool(Name = "get_store_on_sale_product", Title = "Get FFXIV store on sale product")]
        [Description("Get FFXIV store on sale product")]
        public async Task<StoreProductDto> GetStoreOnSaleNewItem()
        {
            return await _storeService.GetStoreOnSaleItem();
        }

        [McpServerTool(Name = "get_store_product", Title = "Get FFXIV store product by category name")]
        [Description("Get FFXIV store on product by category name")]
        public async Task<StoreProductDto?> GetStoreProduct(string name)
        {
            return await _storeService.GetStoreProduct(name);
        }
    }
}
