using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStone.Model.Parseables.CWLS;
using NetStone.Model.Parseables.Linkshell;
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
    public class LinkshellTool(INetStoneService netStone, ILogger<LinkshellTool> logger)
    {
        private readonly INetStoneService _netStoneService = netStone;
        private readonly ILogger<LinkshellTool> _logger = logger;

        [McpServerTool(Name = "get_crossworld_linkshell_information", Title = "Get crossworld linkshell information")]
        [Description("Get crossworld linkshell information")]
        public async Task<LodestoneCrossworldLinkshell?> GetCrossworldLinkshellInformation(
        [Description("The cross world linkshell name.")] string name,
        [Description("The data center (world).")] string world
    )
        {
            var linkshell = await _netStoneService.GetCrossworldLinkshellId(name, world);

            if (linkshell is null) return null;

            if (string.IsNullOrEmpty(linkshell.Id)) return null;

            var linkshellInfo = await _netStoneService.GetCrossworldLinkshell(linkshell.Id);

            return linkshellInfo;
        }

        [McpServerTool(Name = "get_linkshell_information", Title = "Get linkshell information")]
        [Description("Get linkshell information")]
        public async Task<LodestoneLinkshell?> GetLinkshellInformation(
    [Description("The linkshell name.")] string name,
    [Description("The data center (world).")] string world
)
        {
            var linkshell = await _netStoneService.GetLinkshellId(name, world);

            if (linkshell is null) return null;

            if (string.IsNullOrEmpty(linkshell.Id)) return null;

            var linkshellInfo = await _netStoneService.GetLinkshell(linkshell.Id);

            return linkshellInfo;
        }
    }
}
