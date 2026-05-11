using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.Providers.OpenAI;
using ResumeReview.Api.Services.Schemas;

namespace ResumeReview.Api.Services.AtsEngine;

public sealed class OpenAiKeywordExtractionService : IKeywordExtractionService
{
    private const string SchemaName = "ats_keyword_extraction";

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly OpenAiRetryPolicy _retryPolicy;
    private readonly KeywordExtractionEnricher _enricher;
    private readonly ILogger<OpenAiKeywordExtractionService> _logger;

    public OpenAiKeywordExtractionService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        OpenAiRetryPolicy retryPolicy,
        KeywordExtractionEnricher enricher,
        ILogger<OpenAiKeywordExtractionService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _retryPolicy = retryPolicy;
        _enricher = enricher;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async Task<KeywordExtractionResponse> ExtractKeywordsAsync(
        string aiModel,
        string jobDescription,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(jobDescription);
        var requestBody = new
        {
            model = aiModel,
            input = new object[]
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
            text = new
            {
                verbosity = "low",
                format = new
                {
                    type = "json_schema",
                    name = SchemaName,
                    strict = true,
                    schema = AtsKeywordExtractionSchema.Schema
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        try
        {
            _logger.LogInformation(
                "Starting ATS keyword extraction. Model: {Model}. JobDescriptionLength: {JobDescriptionLength}.",
                aiModel,
                jobDescription.Length);

            using var response = await _retryPolicy.SendAsync(
                async token =>
                {
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    return await _httpClient.PostAsync("responses", content, token);
                },
                "ATS keyword extraction",
                cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "OpenAI ATS keyword extraction failed. Status: {Status}. ResponseBodyLength: {ResponseBodyLength}.",
                    response.StatusCode,
                    responseText.Length);

                throw new InvalidOperationException(
                    $"OpenAI ATS keyword extraction failed: {response.StatusCode}.");
            }

            var modelJson = OpenAiResponseParser.ExtractTextOutput(responseText);
            var extraction = JsonSerializer.Deserialize<KeywordExtractionResponse>(
                modelJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException("OpenAI ATS keyword extraction returned an empty result.");

            _logger.LogInformation(
                "ATS keyword extraction succeeded. KeywordCount: {KeywordCount}.",
                extraction.Keywords.Count);

            return _enricher.Enrich(extraction, jobDescription);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "ATS keyword extraction failed. ExceptionType: {ExceptionType}. JobDescriptionLength: {JobDescriptionLength}.",
                ex.GetType().Name,
                jobDescription.Length);

            throw new InvalidOperationException("ATS keyword extraction failed.");
        }
    }

    private static string BuildPrompt(string jobDescription)
    {
        return $$"""
You are a recruitment analyst. Extract ATS resume-search keywords from this job description.

Definition:
A keyword is a verbatim atomic term that appears in the job description that a candidate could naturally include in a resume skills, experience, or projects section.


RULES:

1. EXTRACT ATOMIC JOB DESCRIPTION KEYWORDS EXACTLY AS THEY APPEAR IN THE JOB DESCRIPTION ONLY
   

2. EXCLUDE NON-KEYWORDS
   Do not extract:
   - education or credential requirements unless they are named certifications or degrees candidates would list directly
   - years-of-experience phrases
   - generic soft skills, personality traits, or attitude requirements
   - complete responsibility statements
   - slash-combined requirement bundles
   - company/platform names unless they are real domain/product keywords candidates would put on a resume

3. TIER BY POSITION
   Tier 1: Atomic keywords in the role title, opening summary, required requirements, or core responsibilities
   Tier 2: Atomic keywords in nice-to-have or optional sections
   Tier 3: Atomic keywords loosely mentioned

4. CONTEXT FOR EACH KEYWORD
   For every keyword, capture what it is used for in the role.

5. MARK MUST-HAVE vs NICE-TO-HAVE
   If the JD explicitly says "required", "must have", "essential" -> mark as must_have.
   If the JD explicitly says "preferred", "nice to have", "a plus" -> mark as nice_to_have.
   If neither explicit language exists, infer from context and tier.

6. CAPTURE VARIATIONS
   If the JD uses both abbreviation and full form, include both in variations.
   If only one form exists, use an empty variations array.

7. CATEGORY
   For every keyword, choose exactly one category:
   technical_skill, tool, framework, language, methodology, domain_keyword,
   role_specific_requirement, soft_skill, generic_business_term, other.

Prefer these final keyword categories:
technical_skill, tool, framework, language, methodology, domain_keyword.
Only use other categories when the term is not a useful ATS keyword.

Return ONLY valid JSON with NO additional text or markdown.

Job Description:
{{jobDescription}}
""";
    }
}
