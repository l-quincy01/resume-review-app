using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.Schemas;
using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Services.AtsService.Providers.OpenAI;

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
    }

    public async Task<KeywordExtractionResponse> ExtractKeywordsAsync(
        string apiKey,
        string aiModel,
        string jobDescription,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(jobDescription);
        var input = new object[]
        {
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "input_text", text = prompt }
                }
            }
        };
        var requestBody = OpenAiResponsesRequestFactory.CreateStructuredRequest(
            aiModel,
            input,
            SchemaName,
            AtsKeywordExtractionSchema.Schema,
            _options.KeywordExtractionMaxOutputTokens);

        var json = JsonSerializer.Serialize(requestBody);

        try
        {
            _logger.LogInformation(
                "Starting ATS keyword extraction. Model: {Model}. JobDescriptionLength: {JobDescriptionLength}.",
                aiModel,
                jobDescription.Length);

            var stopwatch = Stopwatch.StartNew();
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
                "ATS keyword extraction",
                cancellationToken);
            stopwatch.Stop();
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = OpenAiErrorInfo.Parse(responseText);
                _logger.LogError(
                    "OpenAI ATS keyword extraction failed. Model: {Model}. Status: {Status}. ElapsedMs: {ElapsedMs}. ResponseBodyLength: {ResponseBodyLength}. ErrorType: {ErrorType}. ErrorCode: {ErrorCode}. ErrorParam: {ErrorParam}. ErrorMessage: {ErrorMessage}.",
                    aiModel,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    responseText.Length,
                    error.Type,
                    error.Code,
                    error.Param,
                    error.Message);

                throw new InvalidOperationException(
                    $"OpenAI ATS keyword extraction failed: {response.StatusCode}.");
            }

            var usage = OpenAiResponseUsageParser.Parse(responseText);
            var modelJson = OpenAiResponseParser.ExtractTextOutput(responseText);
            var extraction = JsonSerializer.Deserialize<KeywordExtractionResponse>(
                modelJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException("OpenAI ATS keyword extraction returned an empty result.");

            _logger.LogInformation(
                "ATS keyword extraction succeeded. Model: {Model}. ElapsedMs: {ElapsedMs}. ResponseBodyLength: {ResponseBodyLength}. OutputLength: {OutputLength}. KeywordCount: {KeywordCount}. InputTokens: {InputTokens}. OutputTokens: {OutputTokens}. TotalTokens: {TotalTokens}. CachedTokens: {CachedTokens}.",
                aiModel,
                stopwatch.ElapsedMilliseconds,
                responseText.Length,
                modelJson.Length,
                extraction.Keywords.Count,
                usage.InputTokens,
                usage.OutputTokens,
                usage.TotalTokens,
                usage.CachedTokens);

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
