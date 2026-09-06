using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface ICommonService
    {
        Task<IEnumerable<DataCenterDto>?> GetDataCenter(CancellationToken cancellationToken = default);
        Task<IEnumerable<WorldDto>?> GetWorlds(CancellationToken cancellationToken = default);
        Task<string?> GetFFXIVTraditionalChineseLockServerStatus(CancellationToken cancellationToken = default);
    }

    public class CommonService : ICommonService
    {
        private readonly ILogger<CommonService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache? _cache;
        private readonly string _endPoint = "https://paissadb.zhu.codes/worlds";
        private readonly string _chtEndPoint = "https://www.ffxiv.com.tw";

        private const string WorldsCacheKey = "ffxiv_worlds_cache";
        private const string DataCentersCacheKey = "ffxiv_datacenters_cache";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

        public CommonService(HttpClient httpClient, ILogger<CommonService> logger, IMemoryCache? cache = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IEnumerable<DataCenterDto>?> GetDataCenter(CancellationToken cancellationToken = default)
        {
            if (_cache != null && _cache.TryGetValue(DataCentersCacheKey, out List<DataCenterDto>? cachedDataCenters) && cachedDataCenters != null)
            {
                return cachedDataCenters;
            }

            try
            {
                var worlds = await GetWorlds(cancellationToken);

                if (worlds is null) return null;

                var dataCenters = worlds
                    .GroupBy(w => new { w.datacenter_id, w.datacenter_name })
                    .Select(g => new DataCenterDto
                    {
                        DatacenterId = g.Key.datacenter_id,
                        DatacenterName = g.Key.datacenter_name
                    })
                    .ToList();

                _cache?.Set(DataCentersCacheKey, dataCenters, CacheDuration);
                return dataCenters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grouping datacenters from worlds list");
                return null;
            }
        }

        public async Task<IEnumerable<WorldDto>?> GetWorlds(CancellationToken cancellationToken = default)
        {
            if (_cache != null && _cache.TryGetValue(WorldsCacheKey, out List<WorldDto>? cachedWorlds) && cachedWorlds != null)
            {
                return cachedWorlds;
            }

            try
            {
                var response = await _httpClient.GetAsync(_endPoint, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var worlds = JsonSerializer.Deserialize<List<WorldDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (worlds != null)
                {
                    _cache?.Set(WorldsCacheKey, worlds, CacheDuration);
                }

                return worlds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching worlds from {Endpoint}", _endPoint);
                return null;
            }
        }

        public async Task<string?> GetFFXIVTraditionalChineseLockServerStatus(CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{_chtEndPoint}/web/news/news_content.aspx?id=GEYXWWLpAA06";

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36");

                using var resp = await _httpClient.SendAsync(req, cancellationToken);
                resp.EnsureSuccessStatusCode();

                var bytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken);

                Encoding enc = Encoding.UTF8;
                var charset = resp.Content.Headers.ContentType?.CharSet;
                if (!string.IsNullOrWhiteSpace(charset))
                {
                    try { enc = Encoding.GetEncoding(charset); } catch { /* fallback UTF-8 */ }
                }

                var html = enc.GetString(bytes);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var sections = doc.DocumentNode.SelectNodes("//section");
                if (sections is null || sections.Count == 0)
                {
                    _logger.LogWarning("No <section> nodes found in CHT server status page.");
                    return null;
                }

                var best = sections
                    .OrderByDescending(s => (s.InnerText ?? "").Trim().Length)
                    .First();

                var text = ExtractTextPreserveStrike(best);

                text = Regex.Replace(text, @"\r\n", "\n");
                text = Regex.Replace(text, @"[ \t]+\n", "\n");
                text = Regex.Replace(text, @"\n{3,}", "\n\n").Trim();

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FFXIV Traditional Chinese lock server status");
                return null;
            }
        }

        static string ExtractTextPreserveStrike(HtmlNode node)
        {
            var sb = new StringBuilder();
            Walk(node, sb, strikeDepth: 0);

            var text = HtmlEntity.DeEntitize(sb.ToString());

            text = Regex.Replace(text, @"\r\n", "\n");
            text = Regex.Replace(text, @"[ \t]+\n", "\n");
            text = Regex.Replace(text, @"\n{3,}", "\n\n").Trim();

            return text;

            static void Walk(HtmlNode n, StringBuilder sb, int strikeDepth)
            {
                if (n.NodeType == HtmlNodeType.Text)
                {
                    var t = ((HtmlTextNode)n).Text;
                    if (!string.IsNullOrWhiteSpace(t))
                    {
                        sb.Append(t);
                    }
                    return;
                }

                if (n.NodeType != HtmlNodeType.Element && n.NodeType != HtmlNodeType.Document)
                    return;

                var name = (n.Name ?? "").ToLowerInvariant();
                var isStrike = name is "s" or "strike" or "del";
                var nextStrikeDepth = strikeDepth + (isStrike ? 1 : 0);

                if (isStrike && strikeDepth == 0)
                    sb.Append("~~");

                if (name is "br")
                    sb.Append('\n');

                foreach (var c in n.ChildNodes)
                    Walk(c, sb, nextStrikeDepth);

                if (name is "p" or "div" or "section" or "li")
                    sb.Append('\n');

                if (isStrike && strikeDepth == 0)
                    sb.Append("~~");
            }
        }
    }
}
