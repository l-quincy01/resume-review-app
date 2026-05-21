using System.Text.Json.Serialization;

namespace ResumeReview.Api.Dtos.Responses;

public sealed class ResumeReviewStreamEnvelope
{
    [JsonIgnore]
    public string EventName { get; set; } = string.Empty;

    public ResumeReviewStreamEvent Data { get; set; } = new();
}

public sealed class ResumeReviewStreamEvent
{
    [JsonPropertyName("section")]
    public string? Section { get; set; }

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }

    [JsonPropertyName("warning")]
    public string? Warning { get; set; }

    [JsonPropertyName("completed_sections")]
    public List<string> CompletedSections { get; set; } = [];

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
