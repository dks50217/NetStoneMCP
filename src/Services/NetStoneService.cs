using NetStone.Model.Parseables.FreeCompany.Members;
using NetStone.Model.Parseables.Search.Character;
using NetStone.Model.Parseables.Search.FreeCompany;
using NetStone.Search.Character;
using NetStone.Search.FreeCompany;
using NetStone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetStone.Model.Parseables.Character.ClassJob;
using NetStone.Model.Parseables.Character;
using NetStone.Model.Parseables.CWLS;
using System.Xml.Linq;
using NetStone.Search.Linkshell;
using NetStone.Model.Parseables.Search.CWLS;

namespace NetStoneMCP.Services
{
    public interface INetStoneService
    {
        Task InitializeAsync();
        Task<LodestoneCharacter?> GetCharacterInfo(string id);
        Task<FreeCompanySearchEntry?> GetFCInformation(string name, string world);
        Task<CharacterClassJob?> GetCharacterClassJob(string id);
        Task<CharacterSearchEntry?> GetCharacterId(string name, string world);
        Task<IEnumerable<FreeCompanyMembersEntry>?> GetFCMembers(string id);
        Task<CrossworldLinkshellSearchEntry?> GetCrossworldLinkshellId(string name, string world);
        Task<LodestoneCrossworldLinkshell?> GetCrossworldLinkshell(string id);
    }

    public class NetStoneService : INetStoneService
    {
        private LodestoneClient? _lodestoneClient;

        public async Task InitializeAsync()
        {
            _lodestoneClient = await LodestoneClient.GetClientAsync();
        }

        public async Task<CharacterSearchEntry?> GetCharacterId(string name, string world)
        {
            if (_lodestoneClient == null)
                throw new Exception("LodestoneClient initialization failed.");

            var searchResponse = await _lodestoneClient.SearchCharacter(new CharacterSearchQuery()
            {
                CharacterName = name,
                World = world
            });

            var lodestoneCharacter =
                searchResponse?.Results
                .FirstOrDefault(entry => entry.Name == name);

            return lodestoneCharacter;
        }

        public async Task<LodestoneCharacter?> GetCharacterInfo(string id)
        {
            if (_lodestoneClient == null)
                throw new Exception("LodestoneClient initialization failed.");

            var character = await _lodestoneClient.GetCharacter(id);

            return character;
        }

        public async Task<FreeCompanySearchEntry?> GetFCInformation(string name, string world)
        {
            if (_lodestoneClient == null)
                throw new Exception("LodestoneClient initialization failed.");

            var searchResponse = await _lodestoneClient.SearchFreeCompany(new FreeCompanySearchQuery()
            {
                Name = name,
                World = world
            });

            var freeCompany = searchResponse?.Results.FirstOrDefault(entry => entry.Name == name);

            return freeCompany;
        }

        public async Task<IEnumerable<FreeCompanyMembersEntry>?> GetFCMembers(string id)
        {
            if (_lodestoneClient == null)
                throw new Exception("LodestoneClient initialization failed.");

            var searchResponse = await _lodestoneClient.GetFreeCompanyMembers(id: id);

            var freeCompanyMembers = searchResponse?.Members;

            return freeCompanyMembers;
        }

        public async Task<CharacterClassJob?> GetCharacterClassJob(string id)
        {
            if (_lodestoneClient == null)
                throw new Exception("LodestoneClient initialization failed.");

            var classJobResponse = await _lodestoneClient.GetCharacterClassJob(id: id);

            return classJobResponse;
        }

        public async Task<CrossworldLinkshellSearchEntry?> GetCrossworldLinkshellId(string name, string world)
        {
            if (_lodestoneClient == null)
                throw new Exception("LodestoneClient initialization failed.");

            var searchResponse = await _lodestoneClient.SearchCrossworldLinkshell(new CrossworldLinkshellSearchQuery()
            {
               DataCenter = world,
               Name = name
            });

            var crossworldLinkshell =
               searchResponse?.Results
               .FirstOrDefault(entry => entry.Name == name);

            return crossworldLinkshell;
        }

        public async Task<LodestoneCrossworldLinkshell?> GetCrossworldLinkshell(string id)
        {
            if (_lodestoneClient == null)
                throw new Exception("LodestoneClient initialization failed.");

            var result = await _lodestoneClient.GetCrossworldLinkshell(id: id);

            return result;
        }
    }
}
