using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AbuseProtection;
using ResumeReview.Api.Services.AtsService.KeywordAnalysis;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;
using ResumeReview.Api.Services.OpenAiApiKeys;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/ats-engine")]
[Route("api/v{version:apiVersion}/ats-engine")]
[ApiVersion("1.0")]
[AbuseProtectionPolicy(AbuseProtectionPolicyNames.ExpensiveAi)]

public sealed class KeywordExtractionController : ControllerBase
{
    private readonly IKeywordExtractionService _keywordExtractionService;
    private readonly OpenAiOptions _openAiOptions;
    private readonly ILogger<KeywordExtractionController> _logger;

    public KeywordExtractionController(
        IKeywordExtractionService keywordExtractionService,
        IOptions<OpenAiOptions> openAiOptions,
        ILogger<KeywordExtractionController> logger)
    {
        _keywordExtractionService = keywordExtractionService;
        _openAiOptions = openAiOptions.Value;
        _logger = logger;
    }

    [HttpPost("keyword-extraction")]
    [ProducesResponseType(typeof(KeywordExtractionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ExtractKeywords(
        [FromBody] KeywordExtractionRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        if (string.IsNullOrWhiteSpace(request.JobDescription))
        {
            return BadRequest(new { message = "Job description is required." });
        }

        if (request.JobDescription.Length > _openAiOptions.MaxJobDescriptionCharacters)
        {
            return BadRequest(new
            {
                message = $"Job description must be {_openAiOptions.MaxJobDescriptionCharacters} characters or fewer."
            });
        }

        if (string.IsNullOrWhiteSpace(request.AiModel))
        {
            return BadRequest(new { message = "AI model is required." });
        }

        if (!_openAiOptions.AllowedModels.Contains(request.AiModel, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Selected AI model is not allowed.",
                allowedModels = _openAiOptions.AllowedModels
            });
        }

        if (!OpenAiApiKeyProvider.TryGetApiKey(this, out var apiKey, out var apiKeyError))
        {
            return apiKeyError!;
        }

        try
        {
            var response = await _keywordExtractionService.ExtractKeywordsAsync(
                apiKey,
                request.AiModel,
                request.JobDescription,
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
                "ATS keyword extraction endpoint failed. Model: {Model}. JobDescriptionLength: {JobDescriptionLength}.",
                request.AiModel,
                request.JobDescription.Length);

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Keyword extraction failed." });
        }
    }
}
