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

        [McpServerTool(Name = "search_huiji_wiki", Title = "Search Huiji Wiki (灰機Wiki / 灰机Wiki)")]
        [Description("搜尋最終幻想14 (FFXIV) 灰機Wiki (Huiji Wiki / 灰机Wiki) 資料庫。當使用者說『用灰機查』、『查灰機』、『灰機』，或需要查詢 FF14 的世界觀背景、任務劇情、副本攻略、機制、NPC、技能、中英文譯名時調用此工具。支援繁體與簡體中文。Search the Chinese FFXIV database.")]
        public async Task<HuijiSearchResultDto?> SearchHuijiWiki(
            [Description("搜尋關鍵字（例如：'伊弗利特'、'拂曉血盟' / '拂晓血盟'、'曉月之終途'、'亞拉戈神典石'、'沙蛾'）。支援繁簡體。")] string keyword,
            [Description("返回結果數量上限 (1-20，預設 5)。")] int limit = 5,
            CancellationToken cancellationToken = default)
        {
            return await _huijiWikiService.SearchArticlesAsync(keyword, limit, cancellationToken);
        }

        [McpServerTool(Name = "get_huiji_wiki_summary", Title = "Get Huiji Wiki Summary (灰機Wiki摘要)")]
        [Description("取得灰機Wiki (Huiji Wiki / 灰机Wiki) 上特定條目的簡介與摘要。當需要快速了解某個 FFXIV 人物、組織、任務、機制概要時使用。")]
        public async Task<HuijiPageDto?> GetHuijiWikiSummary(
            [Description("欲取得摘要的頁面標題或主題（例如：'拂曉血盟'、'亞拉戈神典石'、'伊弗利特'）。支援繁簡體。")] string? title = null,
            [Description("灰機Wiki頁面ID（若已提供標題則可省略）。")] int? page_id = null,
            CancellationToken cancellationToken = default)
        {
            return await _huijiWikiService.GetPageSummaryAsync(title, page_id, cancellationToken);
        }

        [McpServerTool(Name = "get_huiji_wiki_page_section", Title = "Get Huiji Wiki Page or Section Content (灰機Wiki章節詳細內容)")]
        [Description("取得灰機Wiki (Huiji Wiki / 灰机Wiki) 頁面的特定章節完整內容。當摘要資訊不足、或需要詳細攻略、副本機制解說、背景設定長文、掉落列表時使用。")]
        public async Task<HuijiPageDto?> GetHuijiWikiPageSection(
            [Description("頁面標題（例如：'伊弗利特讨伐战'、'拂曉血盟'）。")] string? title = null,
            [Description("灰機Wiki頁面ID（若已提供標題則可省略）。")] int? page_id = null,
            [Description("章節編號（例如：'1', '2', '3'，可由條目中的 available_sections 得知；留空則取得頁首內容）。")] string? section = null,
            CancellationToken cancellationToken = default)
        {
            return await _huijiWikiService.GetPageContentAsync(title, page_id, section, cancellationToken);
        }
    }
}
