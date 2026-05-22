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
public sealed class HeaderValidation : ControllerBase
{
    private readonly IResumeTextExtractor _resumeTextExtractor;
    private readonly IStandardHeaderValidator _standardHeaderValidator;
    private readonly OpenAiOptions _openAiOptions;
    private readonly ILogger<HeaderValidation> _logger;

    public HeaderValidation(
        IResumeTextExtractor resumeTextExtractor,
        IStandardHeaderValidator standardHeaderValidator,
        IOptions<OpenAiOptions> openAiOptions,
        ILogger<HeaderValidation> logger)
    {
        _resumeTextExtractor = resumeTextExtractor;
        _standardHeaderValidator = standardHeaderValidator;
        _openAiOptions = openAiOptions.Value;
        _logger = logger;
    }

    [HttpPost("header-validation")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(HeaderValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateHeaders(
        [FromForm] HeaderValidationRequest request,
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

        await using var stream = request.Resume.OpenReadStream();
        var resumeText = await _resumeTextExtractor.ExtractTextAsync(stream, cancellationToken);

        _logger.LogInformation(
            "ATS header validation extracted resume text. FileName: {FileName}. TextLength: {TextLength}.",
            request.Resume.FileName,
            resumeText.Length);

        return Ok(_standardHeaderValidator.Validate(resumeText));
    }

    private static string FormatBytes(long bytes)
    {
        return $"{bytes / 1024 / 1024}MB";
    }
}
