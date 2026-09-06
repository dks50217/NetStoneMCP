using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using NetStoneMCP.Tests.Helpers;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Services
{
    public class XIVAPIServiceTests
    {
        [Test]
        public async Task GetItemIdByNameAsync_ReturnsResults()
        {
            var dto = new XivapiSearchResultDto
            {
                Results = new List<XivapiResult>
                {
                    new XivapiResult { RowId = 1, Fields = new XivapiItemFields { Name = "Potion" } }
                }
            };
            var json = JsonSerializer.Serialize(dto);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = new NullLogger<XIVAPIService>();
            var service = new XIVAPIService(client, logger);

            var result = await service.GetItemIdByNameAsync("Potion");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, System.Linq.Enumerable.Count(result!));
            Assert.AreEqual("Potion", System.Linq.Enumerable.First(result!).Fields.Name);
        }

        [Test]
        public async Task GetRecipesByItemIdAsync_ReturnsMappedRecipes()
        {
            var dto = new XivapiRecipeSearchResponseDto
            {
                Results = new List<XivapiRecipeSearchEntryDto>
                {
                    new()
                    {
                        RowId = 1440,
                        Fields = new XivapiRecipeFields
                        {
                            AmountResult = 3,
                            ItemResult = new XivapiItemResultRef
                            {
                                Value = 4551,
                                Fields = new XivapiItemFields { Name = "Potion" }
                            },
                            RecipeLevelTable = new XivapiRecipeLevelTableRef
                            {
                                Value = 12,
                                Fields = new XivapiRecipeLevelFields { ClassJobLevel = 12 }
                            },
                            AmountIngredient = new List<int> { 1, 1, 0 },
                            Ingredient = new List<XivapiIngredientRef>
                            {
                                new() { Value = 5361, Fields = new XivapiItemFields { Name = "Water Shard" } },
                                new() { Value = 5056, Fields = new XivapiItemFields { Name = "Distilled Water" } },
                                new() { Value = 0 }
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(dto);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var logger = new NullLogger<XIVAPIService>();
            var service = new XIVAPIService(client, logger);

            var result = await service.GetRecipesByItemIdAsync(4551);

            Assert.IsNotNull(result);
            var list = System.Linq.Enumerable.ToList(result!);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Potion", list[0].Name);
            Assert.AreEqual(4551, list[0].ItemResultTargetID);
            Assert.AreEqual(12, list[0].ClassJobLevel);
            Assert.AreEqual(2, list[0].Ingredients.Count);
            Assert.AreEqual("Water Shard", list[0].Ingredients[0].Name);
            Assert.AreEqual(1, list[0].Ingredients[0].Amount);
            Assert.AreEqual(5361, list[0].Ingredients[0].ID);
        }

        [Test]
        public async Task GetRecipesByItemIdAsync_WhenError_ReturnsNull()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var client = new HttpClient(handler);
            var logger = new NullLogger<XIVAPIService>();
            var service = new XIVAPIService(client, logger);

            var result = await service.GetRecipesByItemIdAsync(9999);

            Assert.IsNull(result);
        }
    }
}
