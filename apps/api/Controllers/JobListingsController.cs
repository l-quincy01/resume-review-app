using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResumeReview.Api.Models;
using ResumeReview.Api.Services.AbuseProtection;
using ResumeReview.Api.Services.JobSearchService.Listings;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/job-listings")]
[Route("api/v{version:apiVersion}/job-listings")]
[ApiVersion("1.0")]
[AbuseProtectionPolicy(AbuseProtectionPolicyNames.ExpensiveAi)]
public class JobListingsController : ControllerBase
{
    private readonly IJobListingsService _jobListingsService;
    private readonly ILogger<JobListingsController> _logger;

    public JobListingsController(
        IJobListingsService jobListingsService,
        ILogger<JobListingsController> logger)
    {
        _jobListingsService = jobListingsService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(JobListings), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobListings>> GetJobListings(
        [FromBody] JobSearchProfile profile,
        CancellationToken cancellationToken)
    {
        if (profile is null)
        {
            return BadRequest("Request body is required.");
        }

        var hasNoTitles = profile.Titles is null || profile.Titles.Count == 0;
        var hasNoKeywords = profile.Keywords is null || profile.Keywords.Count == 0;

        if (hasNoTitles && hasNoKeywords)
        {
            return BadRequest("At least one title or keyword is required.");
        }

        try
        {
            var result = await _jobListingsService.FindJobListingsAsync(
                "gpt-5",
                profile,
                cancellationToken);

            return Ok(result ?? new JobListings { jobListings = [] });
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Job listings endpoint timed out.");
            return Ok(new JobListings { jobListings = [] });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job listings endpoint failed unexpectedly.");
            return Ok(new JobListings { jobListings = [] });
        }
    }
}
