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

namespace NetStoneMCP.Services
{
    public interface INetStoneService
    {
        Task InitializeAsync();
        Task<FreeCompanySearchEntry?> GetFCInformation(string name, string world);
        Task<CharacterClassJob?> GetCharacterClassJob(string id);
        Task<CharacterSearchEntry?> GetCharacterID(string name, string world);
        Task<IEnumerable<FreeCompanyMembersEntry>?> GetFCMembers(string id);
    }

    public class NetStoneService : INetStoneService
    {
        private LodestoneClient? _lodestoneClient;

        public async Task InitializeAsync()
        {
            _lodestoneClient = await LodestoneClient.GetClientAsync();
        }

        public async Task<CharacterSearchEntry?> GetCharacterID(string name, string world)
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
    }
}
