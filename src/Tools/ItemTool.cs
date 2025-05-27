using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStone.Model.Parseables.CWLS;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class ItemTool(IXIVAPIService xivapiService, ILogger<ItemTool> logger)
    {
        private readonly IXIVAPIService _xivapiService = xivapiService;
        private readonly ILogger<ItemTool> _logger = logger;

        [McpServerTool(Name = "get_item_by_name", Title = "Get item by name")]
        [Description("Fetch an item name using XIVAPI")]
        public async Task<string> GetItemByName([Description("Item name.")] string name)
        {
            var result = await _xivapiService.GetItemIdByNameAsync(name);

            if (result is null || !result.Any())
            {
                return JsonSerializer.Serialize(new { error = "Item not found." });
            }

            var items = result.Select(r => new ItemInformationDto
            {
                ItemId = r.RowId,
                Name = r.Fields.Name
            });

            return JsonSerializer.Serialize(items);
        }
    }
}
