using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetStoneMCP.Model
{
    // ==========================================
    // MediaWiki Raw API Response Models
    // ==========================================

    public class HuijiSearchApiResponse
    {
        [JsonPropertyName("query")]
        public HuijiSearchQuery? Query { get; set; }
    }

    public class HuijiSearchQuery
    {
        [JsonPropertyName("searchinfo")]
        public HuijiSearchInfo? SearchInfo { get; set; }

        [JsonPropertyName("search")]
        public List<HuijiSearchRawItem>? Search { get; set; }
    }

    public class HuijiSearchInfo
    {
        [JsonPropertyName("totalhits")]
        public int TotalHits { get; set; }
    }

    public class HuijiSearchRawItem
    {
        [JsonPropertyName("ns")]
        public int Ns { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("pageid")]
        public int PageId { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("wordcount")]
        public int WordCount { get; set; }

        [JsonPropertyName("snippet")]
        public string Snippet { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }
    }

    public class HuijiParseApiResponse
    {
        [JsonPropertyName("parse")]
        public HuijiParseData? Parse { get; set; }

        [JsonPropertyName("error")]
        public HuijiApiError? Error { get; set; }
    }

    public class HuijiApiError
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("info")]
        public string Info { get; set; } = string.Empty;
    }

    public class HuijiParseData
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("pageid")]
        public int PageId { get; set; }

        [JsonPropertyName("sections")]
        public List<HuijiSectionRaw>? Sections { get; set; }

        [JsonPropertyName("wikitext")]
        public HuijiContentRaw? Wikitext { get; set; }

        [JsonPropertyName("text")]
        public HuijiContentRaw? Text { get; set; }
    }

    public class HuijiSectionRaw
    {
        [JsonPropertyName("toclevel")]
        public int TocLevel { get; set; }

        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;

        [JsonPropertyName("line")]
        public string Line { get; set; } = string.Empty;

        [JsonPropertyName("number")]
        public string Number { get; set; } = string.Empty;

        [JsonPropertyName("index")]
        public string Index { get; set; } = string.Empty;

        [JsonPropertyName("anchor")]
        public string? Anchor { get; set; }
    }

    public class HuijiContentRaw
    {
        [JsonPropertyName("*")]
        public string Content { get; set; } = string.Empty;
    }

    public class HuijiExtractsApiResponse
    {
        [JsonPropertyName("query")]
        public HuijiExtractsQuery? Query { get; set; }
    }

    public class HuijiExtractsQuery
    {
        [JsonPropertyName("pages")]
        public Dictionary<string, HuijiExtractPage>? Pages { get; set; }
    }

    public class HuijiExtractPage
    {
        [JsonPropertyName("pageid")]
        public int PageId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("extract")]
        public string? Extract { get; set; }
    }

    // ==========================================
    // MCP Tool Output DTOs
    // ==========================================

    public class HuijiSearchResultDto
    {
        [JsonPropertyName("total_hits")]
        public int TotalHits { get; set; }

        [JsonPropertyName("results")]
        public List<HuijiSearchResultItemDto> Results { get; set; } = new();
    }

    public class HuijiSearchResultItemDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("page_id")]
        public int PageId { get; set; }

        [JsonPropertyName("snippet")]
        public string Snippet { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    public class HuijiSectionSummaryDto
    {
        [JsonPropertyName("index")]
        public string Index { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }

    public class HuijiPageDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("page_id")]
        public int PageId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("section_title")]
        public string? SectionTitle { get; set; }

        [JsonPropertyName("available_sections")]
        public List<HuijiSectionSummaryDto>? AvailableSections { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("is_truncated")]
        public bool IsTruncated { get; set; }
    }
}
