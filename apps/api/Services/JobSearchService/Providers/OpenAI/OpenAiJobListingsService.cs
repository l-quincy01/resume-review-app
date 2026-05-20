using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using ResumeReview.Api.Options;
using ResumeReview.Api.Models;
using ResumeReview.Api.Services.JobSearchService.Listings;
using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Services.JobSearchService.Providers.OpenAI;

public class OpenAiJobListingsService : IJobListingsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiJobListingsService> _logger;
    private readonly OpenAiOptions _options;
    private readonly OpenAiRetryPolicy _retryPolicy;

    public OpenAiJobListingsService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        OpenAiRetryPolicy retryPolicy,
        ILogger<OpenAiJobListingsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        _retryPolicy = retryPolicy;

        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async Task<JobListings> FindJobListingsAsync(
        string aiModel,
        JobSearchProfile profile,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildJobListingsPrompt(profile);

        var requestBody = new Dictionary<string, object?>
        {
            ["model"] = _options.JobListingsModel,
            ["reasoning"] = new
            {
                effort = "medium"
            },
            ["tools"] = new object[]
            {
                new
                {
                    type = "web_search",
                    search_context_size = "medium",
                    user_location = new
                    {
                        type = "approximate",
                        city = "Johannesburg",
                        region = "Gauteng",
                        country = "ZA",
                        timezone = "Africa/Johannesburg"
                    }
                }
            },
            ["input"] = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "input_text", text = prompt }
                    }
                }
            },
            ["text"] = new
            {
                verbosity = "low",
                format = new
                {
                    type = "json_schema",
                    name = "job_listings",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new
                        {
                            jobListings = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    properties = new
                                    {
                                        listingTitle = new { type = "string" },
                                        companyName = new { type = "string" },
                                        locationName = new { type = "string" },
                                        whyItsGreat = new { type = "string" },
                                        listingLink = new { type = "string" },
                                        searchQuery = new { type = "string" }
                                    },
                                    required = new[]
                                    {
                                    "listingTitle",
                                    "companyName",
                                    "locationName",
                                    "whyItsGreat",
                                    "listingLink",
                                    "searchQuery"
                                }
                                }
                            }
                        },
                        required = new[] { "jobListings" }
                    }
                }
            }
        };

        if (OpenAiResponsesRequestFactory.SupportsTemperature(_options.JobListingsModel))
        {
            requestBody["temperature"] = 0;
        }

        var json = JsonSerializer.Serialize(requestBody);
        try
        {
            _logger.LogInformation(
                "Starting job listings search. TitleCount: {TitleCount}, KeywordCount: {KeywordCount}, LocationCount: {LocationCount}.",
                profile.Titles?.Count ?? 0,
                profile.Keywords?.Count ?? 0,
                profile.Locations?.Count ?? 0);

            using var response = await _retryPolicy.SendAsync(
                async token =>
                {
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    return await _httpClient.PostAsync("responses", content, token);
                },
                "job listings search",
                cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = OpenAiErrorInfo.Parse(responseText);
                _logger.LogError(
                    "OpenAI job listings failed. Status: {Status}. ResponseBodyLength: {ResponseBodyLength}. ErrorType: {ErrorType}. ErrorCode: {ErrorCode}. ErrorParam: {ErrorParam}. ErrorMessage: {ErrorMessage}.",
                    response.StatusCode,
                    responseText.Length,
                    error.Type,
                    error.Code,
                    error.Param,
                    error.Message);

                return new JobListings { jobListings = [] };
            }

            var modelJson = ExtractTextOutput(responseText);

            var result = JsonSerializer.Deserialize<JobListings>(
                modelJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result is null)
            {
                _logger.LogError(
                    "Job listings deserialization returned null. ModelJsonLength: {ModelJsonLength}.",
                    modelJson.Length);
                return new JobListings { jobListings = [] };
            }

            _logger.LogInformation("Job listings success. Count: {Count}", result.jobListings?.Count ?? 0);
            result.jobListings = (result.jobListings ?? [])
                .Take(Math.Max(1, _options.MaxJobListings))
                .ToList();

            return result;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Job listings request was cancelled by the caller.");
            return new JobListings { jobListings = [] };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job listings failed unexpectedly.");
            return new JobListings { jobListings = [] };
        }
    }

    private string BuildJobListingsPrompt(JobSearchProfile profile)
    {
        var titles = profile.Titles?.Count > 0
            ? string.Join(", ", profile.Titles)
            : "Software Developer, Backend Developer";

        var keywords = profile.Keywords?.Count > 0
            ? string.Join(", ", profile.Keywords)
            : "Java, Spring Boot, REST API";

        var seniority = string.IsNullOrWhiteSpace(profile.Seniority)
            ? "Junior to Mid-Level"
            : profile.Seniority;

        var locations = profile.Locations?.Count > 0
            ? string.Join(", ", profile.Locations)
            : "Johannesburg, Pretoria, Gauteng, South Africa, Remote South Africa";

        var exclude = profile.Exclude?.Count > 0
            ? string.Join(", ", profile.Exclude)
            : "Senior, Lead, Manager, Principal, Internship";

        return $$"""
Use the derived candidate search profile below and search the web for real, currently open job listings.
Return ONLY valid JSON.

TASK:
Find the {{Math.Max(1, _options.MaxJobListings)}} best real, currently open job listings for this candidate.

DERIVED SEARCH PROFILE:
- Target titles: {{titles}}
- Core keywords: {{keywords}}
- Seniority: {{seniority}}
- Preferred locations: {{locations}}
- Exclude terms: {{exclude}}

Return ONLY valid JSON matching the schema.
""";
    }

    private static string ExtractTextOutput(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);

        var sb = new StringBuilder();

        foreach (var outputItem in doc.RootElement.GetProperty("output").EnumerateArray())
        {
            if (outputItem.GetProperty("type").GetString() != "message")
                continue;

            foreach (var contentItem in outputItem.GetProperty("content").EnumerateArray())
            {
                if (contentItem.GetProperty("type").GetString() == "output_text")
                {
                    sb.Append(contentItem.GetProperty("text").GetString());
                }
            }
        }

        var result = sb.ToString().Trim();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException("No output_text content found.");
        }

        return result;
    }
}
