using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ResumeReview.Api.Models;
using ResumeReview.Api.Services.AbuseProtection;
using ResumeReview.Api.Services.JobSearchService.Listings;
using ResumeReview.Api.Services.OpenAiApiKeys;

namespace ResumeReview.Api.Controllers;

[ApiController]
[Route("api/job-listings")]
[Route("api/v{version:apiVersion}/job-listings")]
[ApiVersion("1.0")]
[AbuseProtectionPolicy(AbuseProtectionPolicyNames.ExpensiveAi)]
public class JobListingsController : ControllerBase
{
    private readonly IJobListingsService _jobListingsService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<JobListingsController> _logger;

    public JobListingsController(
        IJobListingsService jobListingsService,
        IWebHostEnvironment environment,
        ILogger<JobListingsController> logger)
    {
        _jobListingsService = jobListingsService;
        _environment = environment;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(JobListings), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetJobListings(
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

        if (!OpenAiApiKeyProvider.TryGetApiKey(this, out var apiKey, out var apiKeyError))
        {
            return apiKeyError!;
        }

        LogReceivedProfile(profile);

        try
        {
            var result = await _jobListingsService.FindJobListingsAsync(
                apiKey,
                "gpt-5",
                profile,
                cancellationToken);

            return Ok(result ?? new JobListings { jobListings = [] });
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Job listings endpoint timed out.");
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Job listings search failed." });
        }
        catch (JobListingsProviderException ex)
        {
            _logger.LogWarning(
                ex,
                "Job listings provider failed. TitleCount: {TitleCount}. KeywordCount: {KeywordCount}. LocationCount: {LocationCount}.",
                profile.Titles?.Count ?? 0,
                profile.Keywords?.Count ?? 0,
                profile.Locations?.Count ?? 0);

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Job listings search failed." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job listings endpoint failed unexpectedly.");
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Job listings search failed." });
        }
    }

    private void LogReceivedProfile(JobSearchProfile profile)
    {
        var profileHash = HashProfile(profile);

        _logger.LogInformation(
            "Received job listings profile. TitleCount: {TitleCount}. KeywordCount: {KeywordCount}. LocationCount: {LocationCount}. ExcludeCount: {ExcludeCount}. HasSeniority: {HasSeniority}. ProfileHash: {ProfileHash}.",
            profile.Titles?.Count ?? 0,
            profile.Keywords?.Count ?? 0,
            profile.Locations?.Count ?? 0,
            profile.Exclude?.Count ?? 0,
            !string.IsNullOrWhiteSpace(profile.Seniority),
            profileHash);

        if (!_environment.IsProduction())
        {
            _logger.LogInformation(
                "Received job listings profile detail. ProfileHash: {ProfileHash}. Titles: {Titles}. Keywords: {Keywords}. Seniority: {Seniority}. Locations: {Locations}. Exclude: {Exclude}.",
                profileHash,
                FormatList(profile.Titles),
                FormatList(profile.Keywords),
                profile.Seniority,
                FormatList(profile.Locations),
                FormatList(profile.Exclude));
        }
    }

    private static string FormatList(IEnumerable<string>? values)
    {
        return string.Join(", ", values ?? []);
    }

    private static string HashProfile(JobSearchProfile profile)
    {
        var profileJson = JsonSerializer.Serialize(new
        {
            titles = profile.Titles ?? [],
            keywords = profile.Keywords ?? [],
            seniority = profile.Seniority ?? string.Empty,
            locations = profile.Locations ?? [],
            exclude = profile.Exclude ?? []
        });

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(profileJson));
        return Convert.ToHexString(hashBytes)[..12];
    }
}
