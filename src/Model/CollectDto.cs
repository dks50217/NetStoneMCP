using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetStoneMCP.Model
{
    public class CollectSearchResultDto<T>
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("results")]
        public List<T> Results { get; set; } = new();
    }

    public class CollectSourceDto
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("related_type")]
        public string? RelatedType { get; set; }

        [JsonPropertyName("related_id")]
        public int? RelatedId { get; set; }
    }

    public class CollectNamedEntityDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class CollectItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("enhanced_description")]
        public string? EnhancedDescription { get; set; }

        [JsonPropertyName("tooltip")]
        public string? Tooltip { get; set; }

        [JsonPropertyName("patch")]
        public string? Patch { get; set; }

        [JsonPropertyName("item_id")]
        public int? ItemId { get; set; }

        [JsonPropertyName("tradeable")]
        public bool? Tradeable { get; set; }

        [JsonPropertyName("owned")]
        public string? Owned { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("seats")]
        public int? Seats { get; set; }

        [JsonPropertyName("movement")]
        public string? Movement { get; set; }

        [JsonPropertyName("command")]
        public string? Command { get; set; }

        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("category")]
        public CollectNamedEntityDto? Category { get; set; }

        [JsonPropertyName("type")]
        public CollectNamedEntityDto? Type { get; set; }

        [JsonPropertyName("aspect")]
        public CollectNamedEntityDto? Aspect { get; set; }

        [JsonPropertyName("sources")]
        public List<CollectSourceDto> Sources { get; set; } = new();
    }

    public class CollectAchievementDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("points")]
        public int Points { get; set; }

        [JsonPropertyName("patch")]
        public string? Patch { get; set; }

        [JsonPropertyName("owned")]
        public string? Owned { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        [JsonPropertyName("category")]
        public CollectNamedEntityDto? Category { get; set; }

        [JsonPropertyName("type")]
        public CollectNamedEntityDto? Type { get; set; }

        [JsonPropertyName("reward")]
        public CollectAchievementRewardDto? Reward { get; set; }
    }

    public class CollectAchievementRewardDto
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public CollectAchievementTitleDto? Title { get; set; }

        [JsonPropertyName("item")]
        public CollectNamedEntityDto? Item { get; set; }
    }

    public class CollectAchievementTitleDto
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("female_name")]
        public string? FemaleName { get; set; }
    }
}
