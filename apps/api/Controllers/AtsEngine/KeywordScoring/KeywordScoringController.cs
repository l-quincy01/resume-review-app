using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AtsService.ContextualKeywordScoring;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/ats-engine")]

public sealed class KeywordScoringController : ControllerBase
{
    private readonly IResumeTextExtractor _resumeTextExtractor;
    private readonly IKeyWordAnalysisService _contextualKeywordScoringService;
    private readonly OpenAiOptions _openAiOptions;
    private readonly ILogger<KeywordScoringController> _logger;

    public KeywordScoringController(
        IResumeTextExtractor resumeTextExtractor,
        IKeyWordAnalysisService contextualKeywordScoringService,
        IOptions<OpenAiOptions> openAiOptions,
        ILogger<KeywordScoringController> logger)
    {
        _resumeTextExtractor = resumeTextExtractor;
        _contextualKeywordScoringService = contextualKeywordScoringService;
        _openAiOptions = openAiOptions.Value;
        _logger = logger;
    }

    [HttpPost("contextual-keyword-scoring")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(KeywordAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ScoreKeywords(
        [FromForm] KeywordAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Resume == null || request.Resume.Length == 0)
        {
            return BadRequest(new { message = "Resume PDF is required." });
        }

        if (!string.Equals(request.Resume.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only PDF files are allowed." });
        }

        if (request.Resume.Length > _openAiOptions.MaxResumeBytes)
        {
            return BadRequest(new
            {
                message = $"Resume PDF must be {FormatBytes(_openAiOptions.MaxResumeBytes)} or smaller."
            });
        }

        if (string.IsNullOrWhiteSpace(request.KeywordsJson))
        {
            return BadRequest(new { message = "keywords_json is required." });
        }

        if (string.IsNullOrWhiteSpace(request.AiModel))
        {
            return BadRequest(new { message = "ai_model is required." });
        }

        KeywordExtractionResponse keywords;
        try
        {
            keywords = JsonSerializer.Deserialize<KeywordExtractionResponse>(
                request.KeywordsJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new KeywordExtractionResponse();
        }
        catch (JsonException)
        {
            return BadRequest(new { message = "keywords_json must be valid Stage B keyword JSON." });
        }

        if (keywords.Keywords.Count == 0)
        {
            return BadRequest(new { message = "keywords_json must contain at least one keyword." });
        }

        await using var stream = request.Resume.OpenReadStream();
        var resumeText = await _resumeTextExtractor.ExtractTextAsync(stream, cancellationToken);

        try
        {
            var response = await _contextualKeywordScoringService.ScoreKeywordsAsync(
                request.AiModel,
                keywords,
                resumeText,
                cancellationToken);

            return Ok(response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "ATS contextual keyword scoring endpoint failed. Model: {Model}. KeywordCount: {KeywordCount}. ResumeTextLength: {ResumeTextLength}.",
                request.AiModel,
                keywords.Keywords.Count,
                resumeText.Length);

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Contextual keyword scoring failed." });
        }
    }

    private static string FormatBytes(long bytes)
    {
        return $"{bytes / 1024 / 1024}MB";
    }
}
