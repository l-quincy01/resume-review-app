using Microsoft.AspNetCore.Mvc;

namespace ResumeReview.Api.Dtos.Requests;

public sealed class ContextualKeywordScoringRequest
{
    [FromForm(Name = "resume")]
    public IFormFile Resume { get; set; } = default!;

    [FromForm(Name = "keywords_json")]
    public string KeywordsJson { get; set; } = string.Empty;

    [FromForm(Name = "ai_model")]
    public string AiModel { get; set; } = string.Empty;
}
