using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services;
using ResumeReview.Api.Services.AbuseProtection;
using ResumeReview.Api.Services.OpenAiApiKeys;
using ResumeReview.Api.Services.ResumeReview;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/resume-review")]
[Route("api/v{version:apiVersion}/resume-review")]
[ApiVersion("1.0")]
[AbuseProtectionPolicy(AbuseProtectionPolicyNames.ExpensiveAi)]
public class ResumeReviewController : ControllerBase
{
    private readonly IResumeReviewService _resumeReviewService;
    private readonly OpenAiOptions _openAiOptions;
    private static readonly JsonSerializerOptions StreamJsonOptions = new(JsonSerializerDefaults.Web);

    public ResumeReviewController(
        IResumeReviewService resumeReviewService,
        IOptions<OpenAiOptions> openAiOptions)
    {
        _resumeReviewService = resumeReviewService;
        _openAiOptions = openAiOptions.Value;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ResumeReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit([FromForm] ResumeReviewRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

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

        if (!_openAiOptions.AllowedModels.Contains(request.AiModel, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Selected AI model is not allowed.",
                allowedModels = _openAiOptions.AllowedModels
            });
        }

        if (request.JobDescription?.Length > _openAiOptions.MaxJobDescriptionCharacters)
        {
            return BadRequest(new
            {
                message = $"Job description must be {_openAiOptions.MaxJobDescriptionCharacters} characters or fewer."
            });
        }

        if (!OpenAiApiKeyProvider.TryGetApiKey(this, out var apiKey, out var apiKeyError))
        {
            return apiKeyError!;
        }

        var response = await _resumeReviewService.AnalyzeAsync(apiKey, request, cancellationToken);

        return Ok(response);
    }

    [HttpPost("stream")]
    [Consumes("multipart/form-data")]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Stream([FromForm] ResumeReviewRequest request, CancellationToken cancellationToken)
    {
        var validationResult = ValidateRequest(request);
        if (validationResult != null)
        {
            return validationResult;
        }

        if (!OpenAiApiKeyProvider.TryGetApiKey(this, out var apiKey, out var apiKeyError))
        {
            return apiKeyError!;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.Headers.XContentTypeOptions = "nosniff";

        try
        {
            await foreach (var streamEvent in _resumeReviewService.AnalyzeStreamAsync(apiKey, request, cancellationToken))
            {
                await WriteSseEventAsync(streamEvent, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            var failedEvent = new ResumeReviewStreamEnvelope
            {
                EventName = "review_failed",
                Data = new ResumeReviewStreamEvent
                {
                    Warning = "Resume review failed before all sections could be generated.",
                    Timestamp = DateTimeOffset.UtcNow
                }
            };

            await WriteSseEventAsync(failedEvent, cancellationToken);
        }

        return new EmptyResult();
    }

    private static string FormatBytes(long bytes)
    {
        return $"{bytes / 1024 / 1024}MB";
    }

    private IActionResult? ValidateRequest(ResumeReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

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

        if (!_openAiOptions.AllowedModels.Contains(request.AiModel, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Selected AI model is not allowed.",
                allowedModels = _openAiOptions.AllowedModels
            });
        }

        if (request.JobDescription?.Length > _openAiOptions.MaxJobDescriptionCharacters)
        {
            return BadRequest(new
            {
                message = $"Job description must be {_openAiOptions.MaxJobDescriptionCharacters} characters or fewer."
            });
        }

        return null;
    }

    private async Task WriteSseEventAsync(
        ResumeReviewStreamEnvelope streamEvent,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(streamEvent.Data, StreamJsonOptions);

        await Response.WriteAsync($"event: {streamEvent.EventName}\n", cancellationToken);
        await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
