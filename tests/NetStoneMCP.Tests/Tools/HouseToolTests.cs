using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using NetStoneMCP.Tools;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Tools
{
    public class HouseToolTests
    {
        private class FakeCommonService : ICommonService
        {
            public Task<IEnumerable<DataCenterDto>?> GetDataCenter(System.Threading.CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<DataCenterDto>?>(null);
            public Task<string?> GetFFXIVTraditionalChineseLockServerStatus(System.Threading.CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);

            public Task<IEnumerable<WorldDto>?> GetWorlds(System.Threading.CancellationToken cancellationToken = default)
            {
                var worlds = new List<WorldDto>
                {
                    new() { id = 72, name = "Tonberry", datacenter_id = 1, datacenter_name = "Elemental" },
                    new() { id = 69, name = "Bahamut", datacenter_id = 2, datacenter_name = "Gaia" }
                };
                return Task.FromResult<IEnumerable<WorldDto>?>(worlds);
            }
        }

        private class FakePaissaHouseService : IPaissaHouseService
        {
            public Task<IEnumerable<PaissaHouseDto>?> GetHouseList(int id, System.Threading.CancellationToken cancellationToken = default)
            {
                if (id == 72)
                {
                    var list = new List<PaissaHouseDto>
                    {
                        new()
                        {
                            District = "海霧村 (Mist)",
                            Area = "1-1",
                            Type = "個人",
                            Size = "Small",
                            Price = "3,000,000",
                            LastUpdateTime = DateTimeOffset.UtcNow
                        }
                    };
                    return Task.FromResult<IEnumerable<PaissaHouseDto>?>(list);
                }
                return Task.FromResult<IEnumerable<PaissaHouseDto>?>(null);
            }
        }

        [Test]
        public async Task GetHouseInformation_WithWorldAndDataCenter_ReturnsHouseList()
        {
            var commonService = new FakeCommonService();
            var paissaService = new FakePaissaHouseService();
            var tool = new HouseTool(paissaService, commonService, NullLogger<HouseTool>.Instance);

            var result = await tool.GetHouseInformation("Tonberry", "Elemental");

            Assert.IsNotNull(result);
            var list = result!.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("海霧村 (Mist)", list[0].District);
            Assert.AreEqual("1-1", list[0].Area);
        }

        [Test]
        public async Task GetHouseInformation_WithWorldOnly_ResolvesCorrectWorld()
        {
            var commonService = new FakeCommonService();
            var paissaService = new FakePaissaHouseService();
            var tool = new HouseTool(paissaService, commonService, NullLogger<HouseTool>.Instance);

            var result = await tool.GetHouseInformation("Tonberry");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Count());
        }

        [Test]
        public async Task GetHouseInformation_WhenSwapped_StillResolvesWorld()
        {
            var commonService = new FakeCommonService();
            var paissaService = new FakePaissaHouseService();
            var tool = new HouseTool(paissaService, commonService, NullLogger<HouseTool>.Instance);

            // User or LLM swapped world and dataCenter
            var result = await tool.GetHouseInformation("Elemental", "Tonberry");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.Count());
        }

        [Test]
        public async Task GetHouseInformation_WhenWorldNotFound_ReturnsNull()
        {
            var commonService = new FakeCommonService();
            var paissaService = new FakePaissaHouseService();
            var tool = new HouseTool(paissaService, commonService, NullLogger<HouseTool>.Instance);

            var result = await tool.GetHouseInformation("NonExistentWorld");

            Assert.IsNull(result);
        }
    }
}
