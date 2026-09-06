using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class StoreTool(IStoreService storeService, ILogger<StoreTool> logger)
    {
        private readonly IStoreService _storeService = storeService;
        private readonly ILogger<StoreTool> _logger = logger;

        [McpServerTool(Name = "get_store_categories", Title = "Get FFXIV store categories")]
        [Description("Get all product categories available in the FFXIV Online Store.")]
        public async Task<IEnumerable<StoreCategory>?> GetStoreCategories(CancellationToken cancellationToken = default)
        {
            return await _storeService.GetStoreCategories(cancellationToken);
        }

        [McpServerTool(Name = "get_store_new_product", Title = "Get FFXIV store new products")]
        [Description("Get newly released items and products in the FFXIV Online Store.")]
        public async Task<StoreProductDto?> GetStoreNewItem(CancellationToken cancellationToken = default)
        {
            return await _storeService.GetStoreNewItem(cancellationToken: cancellationToken);
        }

        [McpServerTool(Name = "get_store_on_sale_product", Title = "Get FFXIV store on sale products")]
        [Description("Get items and products currently on discount/sale in the FFXIV Online Store.")]
        public async Task<StoreProductDto?> GetStoreOnSaleNewItem(CancellationToken cancellationToken = default)
        {
            return await _storeService.GetStoreOnSaleItem(cancellationToken: cancellationToken);
        }

        [McpServerTool(Name = "get_store_product", Title = "Get FFXIV store products by category name")]
        [Description("Get FFXIV Online Store products under a specific category (e.g. 'Mounts', 'Minions', 'Costumes').")]
        public async Task<StoreProductDto?> GetStoreProduct(
            [Description("The store category name (e.g. 'Mounts', 'Minions', 'Costumes').")] string name,
            CancellationToken cancellationToken = default)
        {
            return await _storeService.GetStoreProduct(name, cancellationToken: cancellationToken);
        }
    }
}
