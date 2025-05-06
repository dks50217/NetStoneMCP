using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStone.Model.Parseables.Character.ClassJob;
using NetStone.Model.Parseables.FreeCompany.Members;
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
    public class FreeCompanyTool(INetStoneService netStone, ILogger<CharacterInformationTool> logger)
    {
        private readonly INetStoneService _netStoneService = netStone;
        private readonly ILogger<CharacterInformationTool> _logger = logger;

        [McpServerTool(Name = "get_fc_information", Title = "Get fc information")]
        [Description("Get free Company information")]
        public async Task<FreeCompanySearchEntry?> GetFCInformation(
        [Description("The free company name.")] string name,
        [Description("The data center (world).")] string world
    )
        {
            var freeCompany = await _netStoneService.GetFCInformation(name, world);

            return freeCompany;
        }

        [McpServerTool(Name = "get_fc_members", Title = "Get fc members")]
        [Description("Get the members of a free company")]
        public async Task<IEnumerable<FreeCompanyMembersEntry>?> GetFCMembers(
        [Description("The free company name.")] string name,
        [Description("The data center (world).")] string world
    )
        {
            var freeCompany = await _netStoneService.GetFCInformation(name, world);

            if (freeCompany is null) return null;

            if (string.IsNullOrEmpty(freeCompany.Id)) return null;

            var members = await _netStoneService.GetFCMembers(freeCompany.Id);

            return members;
        }
    }
}
