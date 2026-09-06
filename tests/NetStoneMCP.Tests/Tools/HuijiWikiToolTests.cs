using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using NetStoneMCP.Tools;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Tools
{
    public class HuijiWikiToolTests
    {
        private class FakeHuijiWikiService : IHuijiWikiService
        {
            public Task<HuijiSearchResultDto?> SearchArticlesAsync(string keyword, int limit = 5, System.Threading.CancellationToken cancellationToken = default)
            {
                if (keyword == "伊弗利特")
                {
                    return Task.FromResult<HuijiSearchResultDto?>(new HuijiSearchResultDto
                    {
                        TotalHits = 1,
                        Results = new List<HuijiSearchResultItemDto>
                        {
                            new() { Title = "伊弗利特", PageId = 123, Snippet = "火神伊弗利特", Url = "https://wiki/123" }
                        }
                    });
                }
                return Task.FromResult<HuijiSearchResultDto?>(null);
            }

            public Task<HuijiPageDto?> GetPageSummaryAsync(string? title = null, int? pageId = null, System.Threading.CancellationToken cancellationToken = default)
            {
                if (title == "伊弗利特")
                {
                    return Task.FromResult<HuijiPageDto?>(new HuijiPageDto
                    {
                        Title = "伊弗利特",
                        PageId = 123,
                        Content = "火神伊弗利特簡介",
                        SectionTitle = "簡介"
                    });
                }
                return Task.FromResult<HuijiPageDto?>(null);
            }

            public Task<HuijiPageDto?> GetPageContentAsync(string? title = null, int? pageId = null, string? section = null, System.Threading.CancellationToken cancellationToken = default)
            {
                if (title == "伊弗利特" && section == "1")
                {
                    return Task.FromResult<HuijiPageDto?>(new HuijiPageDto
                    {
                        Title = "伊弗利特",
                        PageId = 123,
                        SectionTitle = "戰鬥機制",
                        Content = "地獄之火機制說明"
                    });
                }
                return Task.FromResult<HuijiPageDto?>(null);
            }
        }

        [Test]
        public async Task SearchHuijiWiki_ReturnsResults()
        {
            var fakeService = new FakeHuijiWikiService();
            var tool = new HuijiWikiTool(fakeService, NullLogger<HuijiWikiTool>.Instance);

            var result = await tool.SearchHuijiWiki("伊弗利特");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result!.TotalHits);
            Assert.AreEqual("伊弗利特", result.Results[0].Title);
        }

        [Test]
        public async Task GetHuijiWikiSummary_ReturnsSummary()
        {
            var fakeService = new FakeHuijiWikiService();
            var tool = new HuijiWikiTool(fakeService, NullLogger<HuijiWikiTool>.Instance);

            var result = await tool.GetHuijiWikiSummary("伊弗利特");

            Assert.IsNotNull(result);
            Assert.AreEqual("伊弗利特", result!.Title);
            Assert.AreEqual("火神伊弗利特簡介", result.Content);
        }

        [Test]
        public async Task GetHuijiWikiPageSection_ReturnsSectionContent()
        {
            var fakeService = new FakeHuijiWikiService();
            var tool = new HuijiWikiTool(fakeService, NullLogger<HuijiWikiTool>.Instance);

            var result = await tool.GetHuijiWikiPageSection("伊弗利特", section: "1");

            Assert.IsNotNull(result);
            Assert.AreEqual("戰鬥機制", result!.SectionTitle);
            Assert.AreEqual("地獄之火機制說明", result.Content);
        }
    }
}
