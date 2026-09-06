using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using System.ComponentModel;
using System.Threading.Tasks;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class HuijiWikiTool(IHuijiWikiService huijiWikiService, ILogger<HuijiWikiTool> logger)
    {
        private readonly IHuijiWikiService _huijiWikiService = huijiWikiService;
        private readonly ILogger<HuijiWikiTool> _logger = logger;

        [McpServerTool(Name = "search_huiji_wiki", Title = "Search Huiji Wiki (灰机Wiki)")]
        [Description("Search the Chinese FFXIV database (Huiji Wiki / 灰机Wiki) for lore, quests, duties/dungeons, NPCs, bosses, mechanics, items, skills, and translations in Simplified or Traditional Chinese.")]
        public async Task<HuijiSearchResultDto?> SearchHuijiWiki(
            [Description("Search query (e.g. '伊弗利特', '拂晓血盟', '晓月之终途', '亚拉戈神典石', '沙蛾').")] string keyword,
            [Description("Maximum number of results to return (1-20, default 5).")] int limit = 5,
            CancellationToken cancellationToken = default)
        {
            return await _huijiWikiService.SearchArticlesAsync(keyword, limit, cancellationToken);
        }

        [McpServerTool(Name = "get_huiji_wiki_summary", Title = "Get Huiji Wiki Summary")]
        [Description("Get a concise summary and introduction of an FFXIV topic, character, organisation, quest, or mechanic from Huiji Wiki (灰机Wiki).")]
        public async Task<HuijiPageDto?> GetHuijiWikiSummary(
            [Description("Topic or page title to summarize (e.g. '拂晓血盟', '亚拉戈神典石', '伊弗利特').")] string? title = null,
            [Description("Page ID on Huiji Wiki (optional if title is provided).")] int? page_id = null,
            CancellationToken cancellationToken = default)
        {
            return await _huijiWikiService.GetPageSummaryAsync(title, page_id, cancellationToken);
        }

        [McpServerTool(Name = "get_huiji_wiki_page_section", Title = "Get Huiji Wiki Page or Section Content")]
        [Description("Retrieve detailed content of a specific page or section from Huiji Wiki (灰机Wiki). Use this when summary is truncated or when you need full section text (e.g. strategy, background lore, drop list).")]
        public async Task<HuijiPageDto?> GetHuijiWikiPageSection(
            [Description("Page title (e.g. '伊弗利特讨伐战', '拂晓血盟').")] string? title = null,
            [Description("Page ID on Huiji Wiki (optional if title is provided).")] int? page_id = null,
            [Description("Section index number from available_sections (e.g. '1', '2', '3'). Omit to fetch top content.")] string? section = null,
            CancellationToken cancellationToken = default)
        {
            return await _huijiWikiService.GetPageContentAsync(title, page_id, section, cancellationToken);
        }
    }
}
