using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStone.Model.Parseables.CWLS;
using NetStone.Model.Parseables.Search.FreeCompany;
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
    public class LinkshellTool(INetStoneService netStone, ILogger<CharacterInformationTool> logger)
    {
        private readonly INetStoneService _netStoneService = netStone;
        private readonly ILogger<CharacterInformationTool> _logger = logger;

        [McpServerTool(Name = "get_crossworld_linkshell_information", Title = "Get crossworld linkshell information")]
        [Description("Get crossworld linkshell information")]
        public async Task<LodestoneCrossworldLinkshell?> GetCrossworldLinkshellInformation(
        [Description("The linkshell name.")] string name,
        [Description("The data center (world).")] string world
    )
        {
            var linkshell = await _netStoneService.GetCrossworldLinkshellId(name, world);

            if (linkshell is null) return null;

            if (string.IsNullOrEmpty(linkshell.Id)) return null;

            var linkshellInfo = await _netStoneService.GetCrossworldLinkshell(linkshell.Id);

            return linkshellInfo;
        }
    }
}
