using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AtsEngine;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/ats-engine")]
public sealed class AtsKeywordExtractionController : ControllerBase
{
    private readonly IKeywordExtractionService _keywordExtractionService;
    private readonly OpenAiOptions _openAiOptions;
    private readonly ILogger<AtsKeywordExtractionController> _logger;

    public AtsKeywordExtractionController(
        IKeywordExtractionService keywordExtractionService,
        IOptions<OpenAiOptions> openAiOptions,
        ILogger<AtsKeywordExtractionController> logger)
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

        try
        {
            var response = await _keywordExtractionService.ExtractKeywordsAsync(
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
