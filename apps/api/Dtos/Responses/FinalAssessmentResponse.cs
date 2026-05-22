using System.Text.Json.Serialization;

namespace ResumeReview.Api.Dtos.Responses;

public sealed class FinalAssessmentResponse
{
    [JsonPropertyName("job_title")]
    public string JobTitle { get; set; } = string.Empty;

    [JsonPropertyName("ats_readiness_score")]
    public int AtsReadinessScore { get; set; }

    [JsonPropertyName("overall_keyword_score")]
    public int OverallKeywordScore { get; set; }

    [JsonPropertyName("header_quality_score")]
    public int HeaderQualityScore { get; set; }

    [JsonPropertyName("coverage")]
    public FinalAssessmentCoverageResponse Coverage { get; set; } = new();

    [JsonPropertyName("tier_breakdown")]
    public FinalAssessmentTierBreakdownResponse TierBreakdown { get; set; } = new();

    [JsonPropertyName("critical_gaps")]
    public List<FinalAssessmentCriticalGapResponse> CriticalGaps { get; set; } = [];

    [JsonPropertyName("strengths")]
    public List<FinalAssessmentStrengthResponse> Strengths { get; set; } = [];

    [JsonPropertyName("recommendations")]
    public List<FinalAssessmentRecommendationResponse> Recommendations { get; set; } = [];
}

public sealed class FinalAssessmentCoverageResponse
{
    [JsonPropertyName("overall_presence_rate")]
    public int OverallPresenceRate { get; set; }

    [JsonPropertyName("tier_0_coverage")]
    public int Tier0Coverage { get; set; }

    [JsonPropertyName("tier_1_coverage")]
    public int Tier1Coverage { get; set; }

    [JsonPropertyName("must_have_coverage")]
    public int MustHaveCoverage { get; set; }

    [JsonPropertyName("total_keywords_found")]
    public int TotalKeywordsFound { get; set; }

    [JsonPropertyName("total_keywords_expected")]
    public int TotalKeywordsExpected { get; set; }
}

public sealed class FinalAssessmentTierBreakdownResponse
{
    [JsonPropertyName("tier_0")]
    public FinalAssessmentTierBreakdownItemResponse Tier0 { get; set; } = new();

    [JsonPropertyName("tier_1")]
    public FinalAssessmentTierBreakdownItemResponse Tier1 { get; set; } = new();

    [JsonPropertyName("tier_2")]
    public FinalAssessmentTierBreakdownItemResponse Tier2 { get; set; } = new();

    [JsonPropertyName("tier_3")]
    public FinalAssessmentTierBreakdownItemResponse Tier3 { get; set; } = new();
}

public sealed class FinalAssessmentTierBreakdownItemResponse
{
    [JsonPropertyName("average_score")]
    public int AverageScore { get; set; }

    [JsonPropertyName("present")]
    public int Present { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("missing")]
    public List<string> Missing { get; set; } = [];
}

public sealed class FinalAssessmentCriticalGapResponse
{
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    [JsonPropertyName("keyword_type")]
    public string KeywordType { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    [JsonPropertyName("tier")]
    public int Tier { get; set; }

    [JsonPropertyName("requirement")]
    public string Requirement { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}

public sealed class FinalAssessmentStrengthResponse
{
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    [JsonPropertyName("keyword_type")]
    public string KeywordType { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("evidence")]
    public List<KeywordEvidenceResponse> Evidence { get; set; } = [];
}

public sealed class FinalAssessmentRecommendationResponse
{
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    [JsonPropertyName("keyword_type")]
    public string KeywordType { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("issue")]
    public string Issue { get; set; } = string.Empty;

    [JsonPropertyName("suggestion")]
    public string Suggestion { get; set; } = string.Empty;
}
