using Microsoft.AspNetCore.Mvc;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services.AtsService.ContextualKeywordScoring;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Controllers;


[ApiController]
[Route("api/ats-engine")]
public sealed class KeywordAnalysisObject : ControllerBase
{
    private readonly IKeywordScoringService _keywordScoringService;

    public KeywordAnalysisObject(IKeywordScoringService keywordScoringService)
    {
        _keywordScoringService = keywordScoringService;
    }

    [HttpPost("keyword-scoring")]
    [ProducesResponseType(typeof(KeywordScoringResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ScoreKeywords([FromBody] KeywordScoringRequest? request)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        if (request.KeywordScores.Count == 0)
        {
            return BadRequest(new { message = "keyword_scores must contain at least one keyword." });
        }

        foreach (var keyword in request.KeywordScores)
        {
            if (keyword.Tier is < 0 or > 3)
            {
                return BadRequest(new { message = "Keyword tier must be 0, 1, 2, or 3." });
            }

            if (!IsValidRequirement(keyword.Requirement))
            {
                return BadRequest(new { message = "Keyword requirement must be must_have, nice_to_have, or empty." });
            }
        }

        return Ok(_keywordScoringService.Score(new KeywordAnalysisResponse
        {
            KeywordScores = request.KeywordScores
        }));
    }

    private static bool IsValidRequirement(string? requirement)
    {
        return string.IsNullOrWhiteSpace(requirement) ||
            string.Equals(requirement, "must_have", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(requirement, "nice_to_have", StringComparison.OrdinalIgnoreCase);
    }
}
