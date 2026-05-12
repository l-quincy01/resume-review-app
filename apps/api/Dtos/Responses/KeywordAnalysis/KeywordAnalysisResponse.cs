using System.Text.Json.Serialization;

namespace ResumeReview.Api.Dtos.Responses;


public sealed class KeywordAnalysisResponse
{
    [JsonPropertyName("keyword_scores")]
    public List<KeywordAnalysisItemResponse> KeywordScores { get; set; } = [];
}

public sealed class KeywordAnalysisItemResponse
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
}

public sealed class KeywordContextTypeResponse
{
    [JsonPropertyName("has_achievement")]
    public bool HasAchievement { get; set; }

    [JsonPropertyName("has_metric")]
    public bool HasMetric { get; set; }

    [JsonPropertyName("has_action_verb")]
    public bool HasActionVerb { get; set; }

    [JsonPropertyName("in_experience_section")]
    public bool InExperienceSection { get; set; }

    [JsonPropertyName("in_project_section")]
    public bool InProjectSection { get; set; }

    [JsonPropertyName("in_summary_section")]
    public bool InSummarySection { get; set; }
}

public sealed class KeywordEvidenceResponse
{
    [JsonPropertyName("section")]
    public string Section { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("matched_term")]
    public string MatchedTerm { get; set; } = string.Empty;
}
