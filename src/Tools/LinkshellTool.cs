using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStone.Model.Parseables.CWLS;
using NetStone.Model.Parseables.Linkshell;
using NetStoneMCP.Services;
using System.ComponentModel;
using System.Threading.Tasks;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class LinkshellTool(INetStoneService netStone, ILogger<LinkshellTool> logger)
    {
        private readonly INetStoneService _netStoneService = netStone;
        private readonly ILogger<LinkshellTool> _logger = logger;

        [McpServerTool(Name = "get_crossworld_linkshell_information", Title = "Get Cross-World Linkshell information")]
        [Description("Get FFXIV Cross-World Linkshell (CWLS) profile and member list by CWLS name and data center.")]
        public async Task<LodestoneCrossworldLinkshell?> GetCrossworldLinkshellInformation(
            [Description("The cross world linkshell name.")] string name,
            [Description("The data center name (e.g. 'Elemental', 'Mana', 'Aether').")] string dataCenter,
            CancellationToken cancellationToken = default
        )
        {
            var linkshell = await _netStoneService.GetCrossworldLinkshellId(name, dataCenter);

            if (linkshell is null) return null;

            if (string.IsNullOrEmpty(linkshell.Id)) return null;

            return await _netStoneService.GetCrossworldLinkshell(linkshell.Id);
        }

        [McpServerTool(Name = "get_linkshell_information", Title = "Get Linkshell information")]
        [Description("Get FFXIV Linkshell (LS) profile and member list by linkshell name and server / data center.")]
        public async Task<LodestoneLinkshell?> GetLinkshellInformation(
            [Description("The linkshell name.")] string name,
            [Description("The world / server or data center name (e.g. 'Tonberry', 'Elemental').")] string world,
            CancellationToken cancellationToken = default
        )
        {
            var linkshell = await _netStoneService.GetLinkshellId(name, world);

            if (linkshell is null) return null;

            if (string.IsNullOrEmpty(linkshell.Id)) return null;

            return await _netStoneService.GetLinkshell(linkshell.Id);
        }
    }
}
