using Microsoft.Extensions.Logging.Abstractions;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using NetStoneMCP.Tests.Helpers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetStoneMCP.Tests.Services
{
    [TestFixture]
    public class HuijiWikiServiceTests
    {
        [Test]
        public async Task SearchArticlesAsync_ValidKeyword_ReturnsFormattedResults()
        {
            var apiResponse = new HuijiSearchApiResponse
            {
                Query = new HuijiSearchQuery
                {
                    SearchInfo = new HuijiSearchInfo { TotalHits = 1 },
                    Search = new List<HuijiSearchRawItem>
                    {
                        new()
                        {
                            Title = "伊弗利特",
                            PageId = 87807,
                            Snippet = "掌控着“地狱之火炎”力量的<span class=\"searchmatch\">伊弗利特</span>是火焰和愤怒的化身。"
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(apiResponse);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var service = new HuijiWikiService(client, NullLogger<HuijiWikiService>.Instance);

            var result = await service.SearchArticlesAsync("伊弗利特");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.TotalHits);
            Assert.AreEqual(1, result.Results.Count);
            var item = result.Results.First();
            Assert.AreEqual("伊弗利特", item.Title);
            Assert.AreEqual(87807, item.PageId);
            Assert.AreEqual("掌控着“地狱之火炎”力量的伊弗利特是火焰和愤怒的化身。", item.Snippet);
            Assert.AreEqual("https://ff14.huijiwiki.com/wiki/%E4%BC%8A%E5%BC%97%E5%88%A9%E7%89%B9", item.Url);
        }

        [Test]
        public async Task SearchArticlesAsync_EmptyKeyword_ReturnsNull()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var client = new HttpClient(handler);
            var service = new HuijiWikiService(client, NullLogger<HuijiWikiService>.Instance);

            var result = await service.SearchArticlesAsync("  ");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetPageContentAsync_ByTitle_ReturnsParsedPageAndSections()
        {
            var apiResponse = new HuijiParseApiResponse
            {
                Parse = new HuijiParseData
                {
                    Title = "伊弗利特",
                    PageId = 87807,
                    Wikitext = new HuijiContentRaw { Content = "==档案==\n伊弗利特是蜥蜴人族所崇拜的蛮神。" },
                    Sections = new List<HuijiSectionRaw>
                    {
                        new() { Index = "1", Line = "档案", TocLevel = 1 }
                    }
                }
            };

            var json = JsonSerializer.Serialize(apiResponse);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var service = new HuijiWikiService(client, NullLogger<HuijiWikiService>.Instance);

            var result = await service.GetPageContentAsync(title: "伊弗利特");

            Assert.IsNotNull(result);
            Assert.AreEqual("伊弗利特", result!.Title);
            Assert.AreEqual(87807, result.PageId);
            Assert.IsNotNull(result.AvailableSections);
            Assert.AreEqual(1, result.AvailableSections!.Count);
            Assert.AreEqual("档案", result.AvailableSections.First().Title);
            Assert.IsTrue(result.Content!.Contains("伊弗利特是蜥蜴人族所崇拜的蛮神。"));
            Assert.IsFalse(result.IsTruncated);
        }

        [Test]
        public async Task GetPageContentAsync_TemplateTransclusion_ExtractsTextFromHtml()
        {
            var apiResponse = new HuijiParseApiResponse
            {
                Parse = new HuijiParseData
                {
                    Title = "伊弗利特讨伐战",
                    PageId = 87477,
                    Wikitext = new HuijiContentRaw { Content = "{{副本页|id=20001}}" },
                    Text = new HuijiContentRaw
                    {
                        Content = "<div class=\"mw-parser-output\"><p>等级要求：20级</p><div class=\"navbox\">忽略的导航栏</div></div>"
                    }
                }
            };

            var json = JsonSerializer.Serialize(apiResponse);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var service = new HuijiWikiService(client, NullLogger<HuijiWikiService>.Instance);

            var result = await service.GetPageContentAsync(title: "伊弗利特讨伐战");

            Assert.IsNotNull(result);
            Assert.IsTrue(result!.Content!.Contains("等级要求：20级"));
            Assert.IsFalse(result.Content.Contains("忽略的导航栏"));
        }

        [Test]
        public async Task GetPageSummaryAsync_ExtractsAvailable_ReturnsCleanExtract()
        {
            var apiResponse = new HuijiExtractsApiResponse
            {
                Query = new HuijiExtractsQuery
                {
                    Pages = new Dictionary<string, HuijiExtractPage>
                    {
                        ["184454"] = new()
                        {
                            Title = "拂晓血盟",
                            PageId = 184454,
                            Extract = "拂晓血盟是由贤人路易索瓦领导的秘密组织。"
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(apiResponse);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var service = new HuijiWikiService(client, NullLogger<HuijiWikiService>.Instance);

            var result = await service.GetPageSummaryAsync("拂晓血盟");

            Assert.IsNotNull(result);
            Assert.AreEqual("拂晓血盟", result!.Title);
            Assert.AreEqual("拂晓血盟是由贤人路易索瓦领导的秘密组织。", result.Content);
            Assert.AreEqual("簡介 (Summary)", result.SectionTitle);
        }

        [Test]
        public async Task GetPageContentAsync_NotFound_ReturnsNull()
        {
            var apiResponse = new HuijiParseApiResponse
            {
                Error = new HuijiApiError
                {
                    Code = "missingtitle",
                    Info = "The page you specified doesn't exist."
                }
            };

            var json = JsonSerializer.Serialize(apiResponse);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var service = new HuijiWikiService(client, NullLogger<HuijiWikiService>.Instance);

            var result = await service.GetPageContentAsync(title: "不存在的页面XYZ");

            Assert.IsNull(result);
        }
    }
}
