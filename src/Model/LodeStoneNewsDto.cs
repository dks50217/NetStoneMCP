using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class LodeStoneNewsDto
    {

    }

    public class LodeStoneNewsMaintenance
    {
        [JsonPropertyName("companion")]
        public List<LodeStoneNewsItem> Companion { get; set; } = new();

        [JsonPropertyName("game")]
        public List<LodeStoneNewsItem> Game { get; set; } = new();

        [JsonPropertyName("lodestone")]
        public List<LodeStoneNewsItem> Lodestone { get; set; } = new();

        [JsonPropertyName("mog")]
        public List<LodeStoneNewsItem> Mog { get; set; } = new();

        [JsonPropertyName("psn")]
        public List<LodeStoneNewsItem> Psn { get; set; } = new();
    }

    public class LodeStoneNewsItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        [JsonPropertyName("current")]
        public bool Current { get; set; }
    }
}
