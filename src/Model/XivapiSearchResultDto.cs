using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class XivapiSearchResultDto
    {
        [JsonPropertyName("schema")]
        public string Schema { get; set; } = default!;

        [JsonPropertyName("results")]
        public List<XivapiResult> Results { get; set; } = new();
    }

    public class XivapiResult
    {
        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("sheet")]
        public string Sheet { get; set; } = default!;

        [JsonPropertyName("row_id")]
        public int RowId { get; set; }

        [JsonPropertyName("fields")]
        public XivapiItemFields Fields { get; set; } = new();
    }

    public class XivapiItemFields
    {
        [JsonPropertyName("Icon")]
        public XivapiIcon Icon { get; set; } = new();

        [JsonPropertyName("Name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("Singular")]
        public string Singular { get; set; } = default!;
    }

    public class XivapiIcon
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; } = default!;

        [JsonPropertyName("path_hr1")]
        public string PathHr1 { get; set; } = default!;
    }

    public class XivapiRecipeResultDto
    {
        public int ItemResultTargetID { get; set; }
        public string Name { get; set; } = default!;
        public int ClassJobLevel { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; } = new();
    }

    public class RecipeIngredient
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public int Amount { get; set; }
    }

    public class XivapiRecipeSearchResponseDto
    {
        [JsonPropertyName("schema")]
        public string Schema { get; set; } = default!;

        [JsonPropertyName("results")]
        public List<XivapiRecipeSearchEntryDto> Results { get; set; } = new();
    }

    public class XivapiRecipeSearchEntryDto
    {
        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("sheet")]
        public string Sheet { get; set; } = default!;

        [JsonPropertyName("row_id")]
        public int RowId { get; set; }

        [JsonPropertyName("fields")]
        public XivapiRecipeFields Fields { get; set; } = new();
    }

    public class XivapiRecipeFields
    {
        [JsonPropertyName("AmountResult")]
        public int AmountResult { get; set; }

        [JsonPropertyName("ItemResult")]
        public XivapiItemResultRef? ItemResult { get; set; }

        [JsonPropertyName("RecipeLevelTable")]
        public XivapiRecipeLevelTableRef? RecipeLevelTable { get; set; }

        [JsonPropertyName("AmountIngredient")]
        public List<int> AmountIngredient { get; set; } = new();

        [JsonPropertyName("Ingredient")]
        public List<XivapiIngredientRef> Ingredient { get; set; } = new();
    }

    public class XivapiItemResultRef
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("fields")]
        public XivapiItemFields? Fields { get; set; }
    }

    public class XivapiRecipeLevelTableRef
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("fields")]
        public XivapiRecipeLevelFields? Fields { get; set; }
    }

    public class XivapiRecipeLevelFields
    {
        [JsonPropertyName("ClassJobLevel")]
        public int ClassJobLevel { get; set; }
    }

    public class XivapiIngredientRef
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("fields")]
        public XivapiItemFields? Fields { get; set; }
    }
}
