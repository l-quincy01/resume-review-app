using System.Text.Json.Serialization;

namespace ResumeReview.Api.Dtos.Requests;

public sealed class KeywordExtractionRequest
{
    [JsonPropertyName("job_description")]
    public string JobDescription { get; set; } = string.Empty;

    [JsonPropertyName("ai_model")]
    public string AiModel { get; set; } = string.Empty;
}
