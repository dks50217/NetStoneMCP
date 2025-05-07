using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
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

        [McpServerTool(Name = "get_player_info", Title = "Get a character information")]
        [Description("Get a character by its Lodestone ID.")]
        public async Task<CharacterDto?> GetCharacterInformation(
          [Description("The character name.")] string name,
          [Description("The data center (world).")] string world
      )
        {
            var character = await _netStoneService.GetCharacterId(name, world);

            if (character is null) return null;

            if (string.IsNullOrEmpty(character.Id)) return null;

            var characterInfo = await _netStoneService.GetCharacterInfo(character.Id);

            if (characterInfo is null) return null;

            var raceformat = string.Empty;

            if (FFXIVRacesDict.Races.ContainsKey(characterInfo.Race))
            {
                raceformat = FFXIVRacesDict.Races[characterInfo.Race].ChineseName;
            }

            return new CharacterDto()
            {
                Character = characterInfo,
                Race = raceformat
            };
        }

        public async Task<LodestoneCharacter?> GetCharacterRace(
       [Description("The character name.")] string name,
       [Description("The data center (world).")] string world
    )
        {
            var character = await _netStoneService.GetCharacterId(name, world);

            if (character is null) return null;

            if (string.IsNullOrEmpty(character.Id)) return null;

            var characterInfo = await _netStoneService.GetCharacterInfo(character.Id);

            return characterInfo;
        }


        [McpServerTool(Name = "get_player_class_job", Title = "Get a characters' classjob")]
        [Description("Get a characters' classjob information by its Lodestone ID")]
        public async Task<CharacterClassJob?> GetCharacterClassJob(
        [Description("The character name.")] string name,
        [Description("The data center (world).")] string world
    )
        {
            var character = await _netStoneService.GetCharacterId(name, world);

            if (character is null) return null;

            if (string.IsNullOrEmpty(character.Id)) return null;

            var classJob = await _netStoneService.GetCharacterClassJob(character.Id);

            return classJob;
        }

        [McpServerTool(Name = "get_character_mount_count", Title = "Get character mount count")]
        [Description("Get character mount count")]
        public async Task<CharacterMountDto?> GetCharacterMountCount(
        [Description("The character name.")] string name,
        [Description("The data center (world).")] string world
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
