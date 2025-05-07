using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStone.Model.Parseables.Search.FreeCompany;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

        [McpServerTool(Name = "get_house_information", Title = "Get house information")]
        [Description("Get house information")]
        public async Task<IEnumerable<PaissaHouseDto>?> GetHouseInformation(
        [Description("The server name.")] string server,
        [Description("The data center (world).")] string world
    )
        {
            var worlds = await _commonService.GetWorlds();

            if (worlds is null) return null;

            var worldItem = worlds.FirstOrDefault(w => w.datacenter_name.ToUpper() == world.ToUpper() 
                                                    && w.name.ToUpper() == server.ToUpper());
            if (worldItem is null) return null;

            var result = await _paissaHouseService.GetHouseList(worldItem.id);

            return result;
        }
    }
}
