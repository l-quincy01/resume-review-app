using Microsoft.AspNetCore.Mvc;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services.AtsService.KeywordAnalysis;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/ats-engine")]

public sealed class AtsValidatorController : ControllerBase
{
    private readonly IFinalAssessmentService _finalAssessmentService;

    public AtsValidatorController(IFinalAssessmentService finalAssessmentService)
    {
        _finalAssessmentService = finalAssessmentService;
    }

    [HttpPost("final-assessment")]
    [ProducesResponseType(typeof(FinalAssessmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Assess([FromBody] FinalAssessmentRequest? request)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        if (request.HeaderQualityScore is < 0 or > 100)
        {
            return BadRequest(new { message = "header_quality_score must be between 0 and 100." });
        }

        if (request.KeywordScores.Count == 0)
        {
            return BadRequest(new { message = "keyword_scores must contain at least one keyword." });
        }

        return Ok(_finalAssessmentService.Assess(request));
    }
}
