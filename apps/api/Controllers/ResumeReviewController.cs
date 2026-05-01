using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services;
using ResumeReview.Api.Services.ResumeReview;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/resume-review")]
public class ResumeReviewController : ControllerBase
{
    private readonly IResumeReviewService _resumeReviewService;
    private readonly OpenAiOptions _openAiOptions;

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

        var response = await _resumeReviewService.AnalyzeAsync(request, cancellationToken);

        return Ok(response);
    }

    private static string FormatBytes(long bytes)
    {
        return $"{bytes / 1024 / 1024}MB";
    }
}
