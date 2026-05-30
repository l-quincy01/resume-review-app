using Asp.Versioning;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AbuseProtection;
using ResumeReview.Api.Services.AtsService.KeywordAnalysis;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/ats-engine")]
[Route("api/v{version:apiVersion}/ats-engine")]
[ApiVersion("1.0")]
[AbuseProtectionPolicy(AbuseProtectionPolicyNames.ExpensiveAi)]
public sealed class KeywordAnalysisController : ControllerBase
{
    private readonly IResumeTextExtractor _resumeTextExtractor;
    private readonly IKeywordAnalysisService _keywordAnalysisService;
    private readonly OpenAiOptions _openAiOptions;
    private readonly ILogger<KeywordAnalysisController> _logger;

    public KeywordAnalysisController(
        IResumeTextExtractor resumeTextExtractor,
        IKeywordAnalysisService keywordAnalysisService,
        IOptions<OpenAiOptions> openAiOptions,
        ILogger<KeywordAnalysisController> logger)
    {
        _resumeTextExtractor = resumeTextExtractor;
        _keywordAnalysisService = keywordAnalysisService;
        _openAiOptions = openAiOptions.Value;
        _logger = logger;
    }

    [HttpPost("keyword-analysis")]
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
            var response = await _keywordAnalysisService.ScoreKeywordsAsync(
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
                "ATS keyword analysis endpoint failed. Model: {Model}. KeywordCount: {KeywordCount}. ResumeTextLength: {ResumeTextLength}.",
                request.AiModel,
                keywords.Keywords.Count,
                resumeText.Length);

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Keyword analysis failed." });
        }
    }

    private static string FormatBytes(long bytes)
    {
        return $"{bytes / 1024 / 1024}MB";
    }
}
