using Microsoft.AspNetCore.Mvc;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Services;
using ResumeReview.Api.Services.ResumeReview;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/resume-review")]
public class ResumeReviewController : ControllerBase
{
    private readonly IResumeReviewService _resumeReviewService;

    public ResumeReviewController(IResumeReviewService resumeReviewService)
    {
        _resumeReviewService = resumeReviewService;
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

        var response = await _resumeReviewService.AnalyzeAsync(request, cancellationToken);

        return Ok(response);
    }
}