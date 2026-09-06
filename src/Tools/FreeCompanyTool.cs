using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStone.Model.Parseables.FreeCompany.Members;
using NetStone.Model.Parseables.Search.FreeCompany;
using NetStoneMCP.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class FreeCompanyTool(INetStoneService netStone, ILogger<FreeCompanyTool> logger)
    {
        private readonly INetStoneService _netStoneService = netStone;
        private readonly ILogger<FreeCompanyTool> _logger = logger;

        [McpServerTool(Name = "get_fc_information", Title = "Get Free Company information")]
        [Description("Get FFXIV Free Company (FC) summary information by company name and world/server.")]
        public async Task<FreeCompanySearchEntry?> GetFCInformation(
            [Description("The free company name.")] string name,
            [Description("The world / server name (e.g. 'Bahamut', 'Tonberry').")] string world,
            CancellationToken cancellationToken = default
        )
        {
            return await _netStoneService.GetFCInformation(name, world);
        }

        [McpServerTool(Name = "get_fc_members", Title = "Get Free Company members")]
        [Description("Get members of an FFXIV Free Company (FC) by company name and world/server.")]
        public async Task<IEnumerable<FreeCompanyMembersEntry>?> GetFCMembers(
            [Description("The free company name.")] string name,
            [Description("The world / server name (e.g. 'Bahamut', 'Tonberry').")] string world,
            CancellationToken cancellationToken = default
        )
        {
            var freeCompany = await _netStoneService.GetFCInformation(name, world);

            if (freeCompany is null) return null;

            if (string.IsNullOrEmpty(freeCompany.Id)) return null;

            return await _netStoneService.GetFCMembers(freeCompany.Id);
        }
    }
}
