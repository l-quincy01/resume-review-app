using System.Text.Json.Serialization;

namespace ResumeReview.Api.Dtos.Responses;

public sealed class KeywordScoringResponse
{
    [JsonPropertyName("keyword_scores")]
    public List<KeywordScoreObject> KeywordScores { get; set; } = [];
}


public sealed class KeywordScoreObject
{
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    [JsonPropertyName("present")]
    public bool Present { get; set; }

    [JsonPropertyName("tier")]
    public int Tier { get; set; }

    [JsonPropertyName("requirement")]
    public string Requirement { get; set; } = string.Empty;

    [JsonPropertyName("keyword_type")]
    public string KeywordType { get; set; } = string.Empty;

    [JsonPropertyName("frequency")]
    public int Frequency { get; set; }

    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    [JsonPropertyName("variations")]
    public List<string> Variations { get; set; } = [];

    [JsonPropertyName("matched_terms")]
    public List<string> MatchedTerms { get; set; } = [];

    [JsonPropertyName("context_type")]
    public KeywordContextTypeResponse ContextType { get; set; } = new();

    [JsonPropertyName("evidence")]
    public List<KeywordEvidenceResponse> Evidence { get; set; } = [];

    [JsonPropertyName("requirement_multiplier")]
    public decimal RequirementMultiplier { get; set; }

    [JsonPropertyName("context_points")]
    public int ContextPoints { get; set; }

    [JsonPropertyName("keyword_score")]
    public int KeywordScore { get; set; }
}
