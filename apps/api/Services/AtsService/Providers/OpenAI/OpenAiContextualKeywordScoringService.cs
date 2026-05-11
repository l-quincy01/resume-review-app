using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AtsService.ContextualKeywordScoring;
using ResumeReview.Api.Services.AtsService.Schemas;
using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Services.AtsService.Providers.OpenAI;

public sealed class OpenAIKeyWordAnalysisService : IKeyWordAnalysisService
{
    private const string SchemaName = "ats_contextual_keyword_scoring";

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly OpenAiRetryPolicy _retryPolicy;
    private readonly KeywordAnalysisMerger _merger;
    private readonly ILogger<OpenAIKeyWordAnalysisService> _logger;

    public OpenAIKeyWordAnalysisService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        OpenAiRetryPolicy retryPolicy,
        KeywordAnalysisMerger merger,
        ILogger<OpenAIKeyWordAnalysisService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _retryPolicy = retryPolicy;
        _merger = merger;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async Task<KeywordAnalysisResponse> ScoreKeywordsAsync(
        string aiModel,
        KeywordExtractionResponse keywords,
        string resumeText,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(keywords, resumeText);
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
                    schema = AtsContextualKeywordScoringSchema.Schema
                }
            }
        };
        var json = JsonSerializer.Serialize(requestBody);

        try
        {
            _logger.LogInformation(
                "Starting ATS contextual keyword scoring. Model: {Model}. KeywordCount: {KeywordCount}. ResumeTextLength: {ResumeTextLength}.",
                aiModel,
                keywords.Keywords.Count,
                resumeText.Length);

            using var response = await _retryPolicy.SendAsync(
                async token =>
                {
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    return await _httpClient.PostAsync("responses", content, token);
                },
                "ATS contextual keyword scoring",
                cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "OpenAI ATS contextual keyword scoring failed. Status: {Status}. ResponseBodyLength: {ResponseBodyLength}.",
                    response.StatusCode,
                    responseText.Length);

                throw new InvalidOperationException(
                    $"OpenAI ATS contextual keyword scoring failed: {response.StatusCode}.");
            }

            var modelJson = OpenAiResponseParser.ExtractTextOutput(responseText);
            var llmScores = JsonSerializer.Deserialize<KeywordAnalysisResponse>(
                modelJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException("OpenAI ATS contextual keyword scoring returned an empty result.");

            _logger.LogInformation(
                "ATS contextual keyword scoring succeeded. ReturnedKeywordCount: {ReturnedKeywordCount}.",
                llmScores.KeywordScores.Count);

            return _merger.Merge(keywords, llmScores);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "ATS contextual keyword scoring failed. ExceptionType: {ExceptionType}. KeywordCount: {KeywordCount}. ResumeTextLength: {ResumeTextLength}.",
                ex.GetType().Name,
                keywords.Keywords.Count,
                resumeText.Length);

            throw new InvalidOperationException("ATS contextual keyword scoring failed.");
        }
    }

    private static string BuildPrompt(
        KeywordExtractionResponse keywords,
        string resumeText)
    {
        var keywordsJson = JsonSerializer.Serialize(keywords);

        return $$"""
You are an ATS and resume expert. For each keyword provided, scan the entire resume and determine how it is used contextually.

For each keyword, identify all places where it appears and aggregate the context across the entire resume.
Check both the keyword and its variations.

Context type definitions:
- has_achievement: true if the keyword appears in a sentence or bullet that describes an accomplishment or outcome.
- has_metric: true if the keyword appears in a sentence or bullet with numbers, percentages, currency, time, counts, or measurable results.
- has_action_verb: true if the keyword appears in a sentence or bullet that starts with or is paired with an action verb.
- in_experience_section: true if the keyword appears in Work Experience, Professional Experience, Employment History, or similar experience sections.
- in_project_section: true if the keyword appears in Projects, Technical Projects, Selected Projects, or similar project sections.
- in_summary_section: true if the keyword appears in Summary, Professional Summary, Career Summary, Profile, or Objective.

Evidence rules:
- For each keyword that is present, return up to 3 evidence snippets.
- Each evidence snippet must include section, text, and matched_term.
- If the keyword is not present, evidence must be an empty array.
- Do not invent evidence. Use only resume text.

Return ONLY valid JSON with NO additional text or markdown.

JD Keywords:
{{keywordsJson}}

Resume:
{{resumeText}}
""";
    }
}
