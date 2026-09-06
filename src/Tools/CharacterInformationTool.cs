using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStone.Model.Parseables.Character;
using NetStone.Model.Parseables.Character.ClassJob;
using NetStone.Model.Parseables.CWLS;
using NetStoneMCP.Dict;
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
    public class CharacterInformationTool(INetStoneService netStone, ILogger<CharacterInformationTool> logger)
    {
        private readonly INetStoneService _netStoneService = netStone;
        private readonly ILogger<CharacterInformationTool> _logger = logger;

        [McpServerTool(Name = "get_player_info", Title = "Get character profile information")]
        [Description("Get FFXIV character profile details (level, avatar, grand company, free company, bio) by character name and world/server.")]
        public async Task<CharacterDto?> GetCharacterInformation(
          [Description("The character name (e.g. 'Tataru Taru').")] string name,
          [Description("The world / server name (e.g. 'Carbuncle', 'Tonberry').")] string? world,
          CancellationToken cancellationToken = default
      )
        {
            var character = await _netStoneService.GetCharacterId(name, world);

            if (character is null) return null;

            if (string.IsNullOrEmpty(character.Id)) return null;

            var characterInfo = await _netStoneService.GetCharacterInfo(character.Id);

            if (characterInfo is null) return null;

            var raceformat = string.Empty;
            string? clanFormat = null;

            if (FFXIVRacesDict.Races.TryGetValue(characterInfo.Race, out var raceInfo))
            {
                raceformat = raceInfo.ChineseName;
                if (!string.IsNullOrEmpty(characterInfo.Tribe) && raceInfo.Clans.TryGetValue(characterInfo.Tribe, out var clan))
                {
                    clanFormat = clan;
                }
            }

            return new CharacterDto()
            {
                Character = characterInfo,
                Race = raceformat,
                Clan = clanFormat
            };
        }

        [McpServerTool(Name = "get_player_class_job", Title = "Get character class/job levels")]
        [Description("Get all class and job levels of an FFXIV character by character name and world/server.")]
        public async Task<CharacterClassJob?> GetCharacterClassJob(
            [Description("The character name (e.g. 'Tataru Taru').")] string name,
            [Description("The world / server name (e.g. 'Carbuncle', 'Tonberry').")] string? world,
            CancellationToken cancellationToken = default
        )
        {
            var character = await _netStoneService.GetCharacterId(name, world);

            if (character is null) return null;

            if (string.IsNullOrEmpty(character.Id)) return null;

            var classJob = await _netStoneService.GetCharacterClassJob(character.Id);

            return classJob;
        }

        [McpServerTool(Name = "get_character_mount_count", Title = "Get character mount count")]
        [Description("Get total mount count of an FFXIV character by character name and world/server.")]
        public async Task<CharacterMountDto?> GetCharacterMountCount(
            [Description("The character name (e.g. 'Tataru Taru').")] string name,
            [Description("The world / server name (e.g. 'Carbuncle', 'Tonberry').")] string? world,
            CancellationToken cancellationToken = default
        )
        {
            var character = await _netStoneService.GetCharacterId(name, world);

            if (character is null) return null;

            if (string.IsNullOrEmpty(character.Id)) return null;

            var mountInfo = await _netStoneService.GetCharacterMount(character.Id);

            if (mountInfo is null) return null;

            return new CharacterMountDto()
            {
                Count = mountInfo.Collectables.Count()
            };
        }
    }
}
