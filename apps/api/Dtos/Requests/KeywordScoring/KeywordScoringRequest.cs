using System.Text.Json.Serialization;
using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Dtos.Requests;

public sealed class KeywordScoringRequest
{
    [JsonPropertyName("keyword_scores")]
    public List<KeywordAnalysisObject> KeywordScores { get; set; } = [];
}
