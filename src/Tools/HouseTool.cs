using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class HouseTool(IPaissaHouseService paissaHouseService, 
                           ICommonService commonService,
                           ILogger<HouseTool> logger)
    {
        private readonly ICommonService _commonService = commonService;
        private readonly IPaissaHouseService _paissaHouseService = paissaHouseService;
        private readonly ILogger<HouseTool> _logger = logger;

        [McpServerTool(Name = "get_house_information", Title = "Get available housing plots")]
        [Description("Get purchasable housing plot information for a specific FFXIV world / server (e.g. Tonberry, Bahamut). Returns plot area, district, size, price, and lottery entries.")]
        public async Task<IEnumerable<PaissaHouseDto>?> GetHouseInformation(
            [Description("The world / server name (e.g. 'Tonberry', 'Bahamut', 'Carbuncle').")] string world,
            [Description("The data center name (e.g. 'Elemental', 'Gaia', 'Mana', 'Aether'). Optional.")] string? dataCenter = null,
            CancellationToken cancellationToken = default
        )
        {
            var worlds = await _commonService.GetWorlds(cancellationToken);

            if (worlds is null) return null;

            var worldItem = worlds.FirstOrDefault(w =>
                string.Equals(w.name, world, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrWhiteSpace(dataCenter) || string.Equals(w.datacenter_name, dataCenter, StringComparison.OrdinalIgnoreCase)));

            // Fallback in case an LLM swapped world and dataCenter
            if (worldItem is null && !string.IsNullOrWhiteSpace(dataCenter))
            {
                worldItem = worlds.FirstOrDefault(w =>
                    string.Equals(w.name, dataCenter, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(w.datacenter_name, world, StringComparison.OrdinalIgnoreCase));
            }

            if (worldItem is null) return null;

            return await _paissaHouseService.GetHouseList(worldItem.id, cancellationToken);
        }
    }
}
