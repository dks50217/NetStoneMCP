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
    public class ItemToolTests
    {
        private class FakeXIVAPIService : IXIVAPIService
        {
            public Task<IEnumerable<XivapiResult>?> GetItemIdByNameAsync(string itemName, System.Threading.CancellationToken cancellationToken = default)
            {
                if (itemName == "Potion")
                {
                    var results = new List<XivapiResult>
                    {
                        new() { RowId = 4551, Fields = new XivapiItemFields { Name = "Potion" } }
                    };
                    return Task.FromResult<IEnumerable<XivapiResult>?>(results);
                }
                return Task.FromResult<IEnumerable<XivapiResult>?>(null);
            }

            public Task<IEnumerable<XivapiRecipeResultDto>?> GetRecipesByItemIdAsync(int itemId, System.Threading.CancellationToken cancellationToken = default)
            {
                if (itemId == 4551)
                {
                    var recipes = new List<XivapiRecipeResultDto>
                    {
                        new()
                        {
                            ItemResultTargetID = 4551,
                            Name = "Potion",
                            ClassJobLevel = 12,
                            Ingredients = new List<RecipeIngredient>
                            {
                                new() { ID = 5361, Name = "Water Shard", Amount = 1 }
                            }
                        }
                    };
                    return Task.FromResult<IEnumerable<XivapiRecipeResultDto>?>(recipes);
                }
                return Task.FromResult<IEnumerable<XivapiRecipeResultDto>?>(null);
            }
        }

        [Test]
        public async Task GetItemByName_WhenFound_ReturnsStronglyTypedList()
        {
            var fakeService = new FakeXIVAPIService();
            var tool = new ItemTool(fakeService, NullLogger<ItemTool>.Instance);

            var result = await tool.GetItemByName("Potion");

            Assert.IsNotNull(result);
            var list = result!.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(4551, list[0].ItemId);
            Assert.AreEqual("Potion", list[0].Name);
        }

        [Test]
        public async Task GetItemByName_WhenNotFound_ReturnsNull()
        {
            var fakeService = new FakeXIVAPIService();
            var tool = new ItemTool(fakeService, NullLogger<ItemTool>.Instance);

            var result = await tool.GetItemByName("Unknown");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetRecipesByItemId_WhenFound_ReturnsRecipes()
        {
            var fakeService = new FakeXIVAPIService();
            var tool = new ItemTool(fakeService, NullLogger<ItemTool>.Instance);

            var result = await tool.GetRecipesByItemId(4551);

            Assert.IsNotNull(result);
            var list = result!.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Potion", list[0].Name);
            Assert.AreEqual(1, list[0].Ingredients.Count);
        }
    }
}
