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
    public class CommonTool(ICommonService commonService, ILodeStoneNewsService lodeStoneNewsService, ILogger<CommonTool> logger)
    {
        private readonly ICommonService _commonService = commonService;
        private readonly ILodeStoneNewsService _lodeStoneNewsService = lodeStoneNewsService;
        private readonly ILogger<CommonTool> _logger = logger;

        [McpServerTool(Name = "get_data_center", Title = "Get data center information")]
        [Description("Get data center information")]
        public async Task<IEnumerable<DataCenterDto>?> GetDataCenter()
        {
            return await _commonService.GetDataCenter();
        }

        [McpServerTool(Name = "get_world", Title = "Get world information")]
        [Description("Get world information")]
        public async Task<IEnumerable<WorldDto>?> GetWorlds()
        {
            return await _commonService.GetWorlds();
        }

        [McpServerTool(
            Name = "ffxiv_get_current_maintenance",
            Title = "FFXIV Current Maintenance Info"
        )]
        [Description("Retrieve the current Final Fantasy XIV maintenance events, including start time, end time, and description.")]
        public async Task<IEnumerable<LodeStoneNewsMaintenance>?> GetCurrentMaintenance()
        {
            return await _lodeStoneNewsService.GetCurrentMaintenances();
        }
    }
}
