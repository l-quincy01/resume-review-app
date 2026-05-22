using System.Text.Json.Serialization;
using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Dtos.Requests;

public sealed class FinalAssessmentRequest
{
    [JsonPropertyName("job_title")]
    public string JobTitle { get; set; } = string.Empty;

    [JsonPropertyName("header_quality_score")]
    public int HeaderQualityScore { get; set; }

    [JsonPropertyName("keyword_scores")]
    public List<KeywordScoreObject> KeywordScores { get; set; } = [];
}
