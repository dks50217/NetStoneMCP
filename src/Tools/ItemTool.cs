using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class ItemTool(IXIVAPIService xivapiService, ILogger<ItemTool> logger)
    {
        private readonly IXIVAPIService _xivapiService = xivapiService;
        private readonly ILogger<ItemTool> _logger = logger;

        [McpServerTool(Name = "get_item_by_name", Title = "Get item by name")]
        [Description("Search for FFXIV items by name using XIVAPI to find item IDs and English names.")]
        public async Task<IEnumerable<ItemInformationDto>?> GetItemByName(
            [Description("The item name (e.g. 'Potion', 'Excalibur').")] string name,
            CancellationToken cancellationToken = default)
        {
            var result = await _xivapiService.GetItemIdByNameAsync(name, cancellationToken);

            if (result is null) return null;

            return result.Select(r => new ItemInformationDto
            {
                ItemId = r.RowId,
                Name = r.Fields.Name
            }).ToList();
        }

        [McpServerTool(Name = "get_item_recipes", Title = "Get item crafting recipes")]
        [Description("Retrieve all crafting recipes that produce the specified FFXIV item via XIVAPI v2 (returns recipe name, required job level, and ingredients).")]
        public async Task<IEnumerable<XivapiRecipeResultDto>?> GetRecipesByItemId(
            [Description("The FFXIV item ID (e.g. 4551 for Potion).")] int id,
            CancellationToken cancellationToken = default)
        {
            return await _xivapiService.GetRecipesByItemIdAsync(id, cancellationToken);
        }
    }
}
