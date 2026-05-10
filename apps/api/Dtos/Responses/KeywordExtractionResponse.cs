using System.Text.Json.Serialization;

namespace ResumeReview.Api.Dtos.Responses;

public sealed class KeywordExtractionResponse
{
    [JsonPropertyName("job_title")]
    public string JobTitle { get; set; } = string.Empty;

    [JsonPropertyName("keywords")]
    public List<KeywordExtractionItemResponse> Keywords { get; set; } = [];
}

public sealed class KeywordExtractionItemResponse
{
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    [JsonPropertyName("keyword_type")]
    public string KeywordType { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("tier")]
    public int Tier { get; set; }

    [JsonPropertyName("requirement")]
    public string Requirement { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    [JsonPropertyName("variations")]
    public List<string> Variations { get; set; } = [];

    [JsonPropertyName("frequency")]
    public int Frequency { get; set; }

    [JsonPropertyName("boost_applied")]
    public bool BoostApplied { get; set; }
}
