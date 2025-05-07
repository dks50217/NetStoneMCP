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
    public class CommonTool(ICommonService commonService, ILogger<CommonTool> logger)
    {
        private readonly ICommonService _commonService = commonService;
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
    }
}
