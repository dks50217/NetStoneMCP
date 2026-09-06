using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface IHuijiWikiService
    {
        Task<HuijiSearchResultDto?> SearchArticlesAsync(string keyword, int limit = 5, CancellationToken cancellationToken = default);
        Task<HuijiPageDto?> GetPageContentAsync(string? title = null, int? pageId = null, string? section = null, CancellationToken cancellationToken = default);
        Task<HuijiPageDto?> GetPageSummaryAsync(string? title = null, int? pageId = null, CancellationToken cancellationToken = default);
    }

    public class HuijiWikiService : IHuijiWikiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HuijiWikiService> _logger;
        private readonly IMemoryCache? _cache;
        private readonly string _apiBase = "https://ff14.huijiwiki.com/api.php";
        private readonly string _wikiBase = "https://ff14.huijiwiki.com/wiki/";
        private const int MaxContentLength = 2000;

        public HuijiWikiService(HttpClient httpClient, ILogger<HuijiWikiService> logger, IMemoryCache? cache = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<HuijiSearchResultDto?> SearchArticlesAsync(string keyword, int limit = 5, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return null;
            }

            var clampedLimit = Math.Clamp(limit, 1, 20);
            var cacheKey = $"Huiji_Search_{keyword.Trim().ToLowerInvariant()}_{clampedLimit}";
            if (_cache != null && _cache.TryGetValue(cacheKey, out HuijiSearchResultDto? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var url = $"{_apiBase}?action=query&list=search&srsearch={Uri.EscapeDataString(keyword)}&srlimit={clampedLimit}&format=json";

                var response = await _httpClient.GetFromJsonAsync<HuijiSearchApiResponse>(url, cancellationToken);
                if (response?.Query?.Search is null)
                {
                    return new HuijiSearchResultDto { TotalHits = 0, Results = new() };
                }

                var items = response.Query.Search.Select(s => new HuijiSearchResultItemDto
                {
                    Title = s.Title,
                    PageId = s.PageId,
                    Snippet = CleanSnippet(s.Snippet),
                    Url = $"{_wikiBase}{Uri.EscapeDataString(s.Title)}"
                }).ToList();

                var result = new HuijiSearchResultDto
                {
                    TotalHits = response.Query.SearchInfo?.TotalHits ?? items.Count,
                    Results = items
                };

                _cache?.Set(cacheKey, result, TimeSpan.FromMinutes(30));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Huiji Wiki for keyword: {Keyword}", keyword);
                return null;
            }
        }

        public async Task<HuijiPageDto?> GetPageContentAsync(string? title = null, int? pageId = null, string? section = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(title) && !pageId.HasValue)
            {
                return null;
            }

            var cacheKey = $"Huiji_Content_{title ?? string.Empty}_{pageId}_{section ?? string.Empty}";
            if (_cache != null && _cache.TryGetValue(cacheKey, out HuijiPageDto? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var targetParam = pageId.HasValue
                    ? $"pageid={pageId.Value}"
                    : $"page={Uri.EscapeDataString(title!)}";

                var sectionParam = !string.IsNullOrWhiteSpace(section)
                    ? $"&section={Uri.EscapeDataString(section)}"
                    : string.Empty;

                var url = $"{_apiBase}?action=parse&{targetParam}&prop=sections|wikitext|text&redirects=1{sectionParam}&format=json";

                var response = await _httpClient.GetFromJsonAsync<HuijiParseApiResponse>(url, cancellationToken);
                if (response?.Parse is null)
                {
                    if (response?.Error is not null)
                    {
                        _logger.LogWarning("Huiji API parse returned error: {Code} - {Info}", response.Error.Code, response.Error.Info);
                    }
                    return null;
                }

                var parse = response.Parse;
                var pageDto = new HuijiPageDto
                {
                    Title = parse.Title,
                    PageId = parse.PageId,
                    Url = $"{_wikiBase}{Uri.EscapeDataString(parse.Title)}",
                    AvailableSections = parse.Sections?.Select(s => new HuijiSectionSummaryDto
                    {
                        Index = s.Index,
                        Level = s.TocLevel,
                        Title = WebUtility.HtmlDecode(s.Line)
                    }).ToList()
                };

                if (!string.IsNullOrWhiteSpace(section) && pageDto.AvailableSections != null)
                {
                    var matchedSection = pageDto.AvailableSections.FirstOrDefault(s => s.Index == section);
                    pageDto.SectionTitle = matchedSection?.Title;
                }

                var content = ResolveContent(parse);
                if (!string.IsNullOrEmpty(content) && content.Length > MaxContentLength && string.IsNullOrWhiteSpace(section))
                {
                    pageDto.Content = content[..MaxContentLength] + "\n\n...[內容已截斷。可利用 section 參數查詢特定章節內容]...";
                    pageDto.IsTruncated = true;
                }
                else
                {
                    pageDto.Content = content;
                    pageDto.IsTruncated = false;
                }

                _cache?.Set(cacheKey, pageDto, TimeSpan.FromHours(1));
                return pageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Huiji Wiki page content for title: {Title}, pageId: {PageId}", title, pageId);
                return null;
            }
        }

        public async Task<HuijiPageDto?> GetPageSummaryAsync(string? title = null, int? pageId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(title) && !pageId.HasValue)
            {
                return null;
            }

            var cacheKey = $"Huiji_Summary_{title ?? string.Empty}_{pageId}";
            if (_cache != null && _cache.TryGetValue(cacheKey, out HuijiPageDto? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                var targetParam = pageId.HasValue
                    ? $"pageids={pageId.Value}"
                    : $"titles={Uri.EscapeDataString(title!)}";

                var url = $"{_apiBase}?action=query&prop=extracts&exintro=1&explaintext=1&redirects=1&{targetParam}&format=json";

                var response = await _httpClient.GetFromJsonAsync<HuijiExtractsApiResponse>(url, cancellationToken);
                var page = response?.Query?.Pages?.Values.FirstOrDefault();

                if (page != null && page.PageId > 0 && !string.IsNullOrWhiteSpace(page.Extract))
                {
                    var summaryDto = new HuijiPageDto
                    {
                        Title = page.Title,
                        PageId = page.PageId,
                        Url = $"{_wikiBase}{Uri.EscapeDataString(page.Title)}",
                        Content = page.Extract.Trim(),
                        SectionTitle = "簡介 (Summary)",
                        IsTruncated = false
                    };
                    _cache?.Set(cacheKey, summaryDto, TimeSpan.FromHours(1));
                    return summaryDto;
                }

                // Fallback: fetch lead section (section 0)
                var leadPage = await GetPageContentAsync(title, pageId, section: "0", cancellationToken);
                if (leadPage != null && !string.IsNullOrWhiteSpace(leadPage.Content))
                {
                    leadPage.SectionTitle = "導言/簡介";
                    _cache?.Set(cacheKey, leadPage, TimeSpan.FromHours(1));
                    return leadPage;
                }

                // If lead section is empty, fetch section 1 (e.g. 檔案 / 基本信息)
                var section1Page = await GetPageContentAsync(title, pageId, section: "1", cancellationToken);
                if (section1Page != null && !string.IsNullOrWhiteSpace(section1Page.Content))
                {
                    _cache?.Set(cacheKey, section1Page, TimeSpan.FromHours(1));
                    return section1Page;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Huiji Wiki summary for title: {Title}, pageId: {PageId}", title, pageId);
                return null;
            }
        }

        private static string CleanSnippet(string? snippet)
        {
            if (string.IsNullOrWhiteSpace(snippet))
            {
                return string.Empty;
            }

            // Replace search highlight spans with bold markers
            var cleaned = Regex.Replace(snippet, @"<span class=""searchmatch"">(.*?)</span>", "$1", RegexOptions.IgnoreCase);
            // Remove any other HTML tags
            cleaned = Regex.Replace(cleaned, @"<[^>]+>", string.Empty);
            return WebUtility.HtmlDecode(cleaned).Trim();
        }

        private static string ResolveContent(HuijiParseData parse)
        {
            var wikitext = parse.Wikitext?.Content?.Trim() ?? string.Empty;

            // If wikitext is minimal (e.g. only template transclusion like {{副本页|id=...}})
            // and rendered text is available, extract text from HTML using HtmlAgilityPack
            if (wikitext.Length < 100 && wikitext.StartsWith("{{") && !string.IsNullOrWhiteSpace(parse.Text?.Content))
            {
                var htmlText = ExtractCleanTextFromHtml(parse.Text.Content);
                if (!string.IsNullOrWhiteSpace(htmlText))
                {
                    return htmlText;
                }
            }

            return wikitext;
        }

        private static string ExtractCleanTextFromHtml(string html)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Remove unwanted nodes (navigation boxes, scripts, styles, TOC)
                var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//div[contains(@class, 'navbox')]|//table[contains(@class, 'navbox')]|//div[contains(@class, 'navigation-not-searchable')]|//div[contains(@class, 'toc')]");
                if (nodesToRemove != null)
                {
                    foreach (var node in nodesToRemove)
                    {
                        node.Remove();
                    }
                }

                var text = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
                // Collapse excessive whitespace and blank lines
                text = Regex.Replace(text, @"[ \t]+", " ");
                text = Regex.Replace(text, @"(\r?\n\s*){3,}", "\n\n");
                return text.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
