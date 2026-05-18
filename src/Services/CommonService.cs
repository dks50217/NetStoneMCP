using HtmlAgilityPack;
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
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface ICommonService
    {
        Task<IEnumerable<DataCenterDto>?> GetDataCenter();
        Task<IEnumerable<WorldDto>?> GetWorlds();
        Task<string?> GetFFXIVTraditionalChineseLockServerStatus();
    }

    public class CommonService : ICommonService
    {
        private readonly ILogger<CommonService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _endPoint = "https://paissadb.zhu.codes/worlds";
        private readonly string _chtEndPoint = "https://www.ffxiv.com.tw";

        public CommonService(HttpClient httpClient, ILogger<CommonService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<DataCenterDto>?> GetDataCenter()
        {
            try
            {
                var response = await _httpClient.GetAsync(_endPoint);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var worlds = JsonSerializer.Deserialize<List<WorldDto>>(content);

                var dataCenters = worlds?
                    .GroupBy(w => new { w.datacenter_id, w.datacenter_name })
                    .Select(g => new DataCenterDto
                    {
                        DatacenterId = g.Key.datacenter_id,
                        DatacenterName = g.Key.datacenter_name
                    })
                    .ToList();

                return dataCenters;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetFFXIVTraditionalChineseLockServerStatus()
        {
            try
            {
                var url = $"{_chtEndPoint}/web/news/news_content.aspx?id=GEYXWWLpAA06";

                // var outputPath = Path.Combine(AppContext.BaseDirectory, "section.txt");

                // 有些站會擋沒有 UA 的 request，先加上比較保險
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36");

                using var resp = await _httpClient.GetAsync(url);
                resp.EnsureSuccessStatusCode();

                var bytes = await resp.Content.ReadAsByteArrayAsync();

                Encoding enc = Encoding.UTF8;
                var charset = resp.Content.Headers.ContentType?.CharSet;
                if (!string.IsNullOrWhiteSpace(charset))
                {
                    try { enc = Encoding.GetEncoding(charset); } catch { /* fallback UTF-8 */ }
                }

                var html = enc.GetString(bytes);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 抓所有 <section>
                var sections = doc.DocumentNode.SelectNodes("//section");
                if (sections is null || sections.Count == 0)
                    throw new Exception("");

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
                _logger.LogError(ex.Message);
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
                        // 保留原本的空白，但避免連續塞一堆
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

        public async Task<IEnumerable<WorldDto>?> GetWorlds()
        {
            try
            {
                var response = await _httpClient.GetAsync(_endPoint);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var worlds = JsonSerializer.Deserialize<List<WorldDto>>(content);

                return worlds;
            }
            catch
            {
                return null;
            }
        }
    }
}
