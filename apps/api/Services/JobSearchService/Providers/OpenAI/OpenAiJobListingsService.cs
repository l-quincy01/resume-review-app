using System.Net.Http.Headers;
using System.Security.Cryptography;
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
    }

    public async Task<JobListings> FindJobListingsAsync(
        string apiKey,
        string aiModel,
        JobSearchProfile profile,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aiModel))
        {
            throw new ArgumentException("AI model is required.", nameof(aiModel));
        }

        var prompt = BuildJobListingsPrompt(profile);

        var requestBody = new Dictionary<string, object?>
        {
            ["model"] = aiModel,
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
            ["tool_choice"] = "required",
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
                verbosity = OpenAiResponsesRequestFactory.ResolveTextVerbosity(aiModel),
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

        if (OpenAiResponsesRequestFactory.SupportsReasoning(aiModel))
        {
            requestBody["reasoning"] = new
            {
                effort = ResolveWebSearchReasoningEffort(aiModel)
            };
        }

        if (OpenAiResponsesRequestFactory.SupportsTemperature(aiModel))
        {
            requestBody["temperature"] = 0;
        }

        var json = JsonSerializer.Serialize(requestBody);
        try
        {
            _logger.LogInformation(
                "Starting job listings search. Model: {Model}. TitleCount: {TitleCount}. KeywordCount: {KeywordCount}. LocationCount: {LocationCount}. ExcludeCount: {ExcludeCount}. HasSeniority: {HasSeniority}. ProfileHash: {ProfileHash}.",
                aiModel,
                profile.Titles?.Count ?? 0,
                profile.Keywords?.Count ?? 0,
                profile.Locations?.Count ?? 0,
                profile.Exclude?.Count ?? 0,
                !string.IsNullOrWhiteSpace(profile.Seniority),
                HashProfile(profile));

            using var response = await _retryPolicy.SendAsync(
                async token =>
                {
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    using var request = new HttpRequestMessage(HttpMethod.Post, "responses")
                    {
                        Content = content
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    return await _httpClient.SendAsync(request, token);
                },
                "job listings search",
                cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = OpenAiErrorInfo.Parse(responseText);
                _logger.LogError(
                    "OpenAI job listings failed. Status: {Status}. ResponseBodyLength: {ResponseBodyLength}. ErrorType: {ErrorType}. ErrorCode: {ErrorCode}. ErrorParam: {ErrorParam}.",
                    response.StatusCode,
                    responseText.Length,
                    error.Type,
                    error.Code,
                    error.Param);

                throw new JobListingsProviderException(
                    $"OpenAI job listings failed: {response.StatusCode}.");
            }

            var modelJson = ExtractTextOutput(responseText);
            _logger.LogInformation(
                "OpenAI job listings response parsed. Model: {Model}. Status: {Status}. ResponseBodyLength: {ResponseBodyLength}. OutputLength: {OutputLength}. ProfileHash: {ProfileHash}.",
                aiModel,
                response.StatusCode,
                responseText.Length,
                modelJson.Length,
                HashProfile(profile));

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
                throw new JobListingsProviderException("OpenAI job listings returned an empty result.");
            }

            _logger.LogInformation(
                "Job listings success. Model: {Model}. OutputLength: {OutputLength}. Count: {Count}. ProfileHash: {ProfileHash}.",
                aiModel,
                modelJson.Length,
                result.jobListings?.Count ?? 0,
                HashProfile(profile));
            result.jobListings = (result.jobListings ?? [])
                .Take(Math.Max(1, _options.MaxJobListings))
                .ToList();

            return result;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Job listings request was cancelled by the caller.");
            throw new JobListingsProviderException("Job listings request was cancelled.", ex);
        }
        catch (JobListingsProviderException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job listings failed unexpectedly.");
            throw new JobListingsProviderException("Job listings failed unexpectedly.", ex);
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

    private static string ResolveWebSearchReasoningEffort(string model)
    {
        return model.StartsWith("gpt-5", StringComparison.OrdinalIgnoreCase)
            ? "low"
            : OpenAiResponsesRequestFactory.ResolveReasoningEffort(model);
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
