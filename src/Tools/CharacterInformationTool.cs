using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStone.Model.Parseables.Character.ClassJob;
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
    public class CharacterInformationTool(INetStoneService netStone, ILogger<CharacterInformationTool> logger)
    {
        private readonly INetStoneService _netStoneService = netStone;
        private readonly ILogger<CharacterInformationTool> _logger = logger;

        [McpServerTool(Name = "get_player_class_job", Title = "Get a characters' classjob")]
        [Description("Get a characters' classjob information by its Lodestone ID")]
        public async Task<CharacterClassJob?> GetCharacterClassJob(
        [Description("The character name.")] string name,
        [Description("The data center (world).")] string world
    )
        {
            var character = await _netStoneService.GetCharacterID(name, world);

            if (character is null) return null;

            if (string.IsNullOrEmpty(character.Id)) return null;

            var classJob = await _netStoneService.GetCharacterClassJob(character.Id);

            return classJob;
        }
    }
}
