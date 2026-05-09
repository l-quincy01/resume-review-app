using System.Text.Json.Serialization;

namespace ResumeReview.Api.Dtos.Responses;

public sealed class HeaderValidationResponse
{
    [JsonPropertyName("header_quality_score")]
    public int HeaderQualityScore { get; set; }

    [JsonPropertyName("headers_found")]
    public List<string> HeadersFound { get; set; } = [];

    [JsonPropertyName("headers_missing")]
    public List<string> HeadersMissing { get; set; } = [];

    [JsonPropertyName("unclear_headers")]
    public List<string> UnclearHeaders { get; set; } = [];

    [JsonPropertyName("non_standard_headers")]
    public List<NonStandardHeaderResponse> NonStandardHeaders { get; set; } = [];

    [JsonPropertyName("structure_quality")]
    public string StructureQuality { get; set; } = "weak";
}

public sealed class NonStandardHeaderResponse
{
    [JsonPropertyName("header_found")]
    public string HeaderFound { get; set; } = string.Empty;

    [JsonPropertyName("mapped_to")]
    public string MappedTo { get; set; } = string.Empty;

    [JsonPropertyName("recommended_header")]
    public string RecommendedHeader { get; set; } = string.Empty;
}
