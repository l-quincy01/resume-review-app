using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AtsService.KeywordAnalysis;
using ResumeReview.Api.Services.AtsService.Schemas;
using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Services.AtsService.Providers.OpenAI;

public sealed class OpenAiKeywordAnalysisService : IKeywordAnalysisService
{
    public const string SchemaName = "ats_contextual_keyword_scoring";

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly OpenAiRetryPolicy _retryPolicy;
    private readonly KeywordAnalysisMerger _merger;
    private readonly ILogger<OpenAiKeywordAnalysisService> _logger;

    public OpenAiKeywordAnalysisService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        OpenAiRetryPolicy retryPolicy,
        KeywordAnalysisMerger merger,
        ILogger<OpenAiKeywordAnalysisService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _retryPolicy = retryPolicy;
        _merger = merger;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
    }

    public async Task<KeywordAnalysisResponse> ScoreKeywordsAsync(
        string apiKey,
        string aiModel,
        KeywordExtractionResponse keywords,
        string resumeText,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Starting ATS contextual keyword scoring. Model: {Model}. KeywordCount: {KeywordCount}. ResumeTextLength: {ResumeTextLength}. BatchSize: {BatchSize}.",
                aiModel,
                keywords.Keywords.Count,
                resumeText.Length,
                GetBatchSize());

            var batchResponses = new List<KeywordAnalysisResponse>();
            var batches = keywords.Keywords
                .Chunk(GetBatchSize())
                .Select((batch, index) => new KeywordExtractionResponse
                {
                    JobTitle = keywords.JobTitle,
                    Keywords = batch.ToList()
                })
                .ToList();

            for (var index = 0; index < batches.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var batchNumber = index + 1;
                var batch = batches[index];

                try
                {
                    var batchResponse = await ScoreKeywordBatchAsync(
                        aiModel,
                        apiKey,
                        batch,
                        resumeText,
                        batchNumber,
                        batches.Count,
                        cancellationToken);

                    batchResponses.Add(batchResponse);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (OpenAiStructuredOutputException ex)
                {
                    _logger.LogWarning(
                        "ATS contextual keyword scoring batch returned malformed JSON. Model: {Model}. Schema: {SchemaName}. BatchNumber: {BatchNumber}. KeywordCount: {KeywordCount}. ResponseBodyLength: {ResponseBodyLength}. OutputLength: {OutputLength}. JsonPath: {JsonPath}. LineNumber: {LineNumber}. BytePositionInLine: {BytePositionInLine}. LikelyTruncated: {LikelyTruncated}.",
                        ex.Model,
                        ex.SchemaName,
                        ex.BatchNumber,
                        ex.KeywordCount,
                        ex.ResponseBodyLength,
                        ex.OutputLength,
                        ex.JsonPath,
                        ex.LineNumber,
                        ex.BytePositionInLine,
                        ex.LikelyTruncated);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        "ATS contextual keyword scoring batch failed. ExceptionType: {ExceptionType}. Model: {Model}. BatchNumber: {BatchNumber}. KeywordCount: {KeywordCount}.",
                        ex.GetType().Name,
                        aiModel,
                        batchNumber,
                        batch.Keywords.Count);
                }
            }

            if (batchResponses.Count == 0)
            {
                throw new InvalidOperationException("All ATS contextual keyword scoring batches failed.");
            }

            var combinedLlmResponse = new KeywordAnalysisResponse
            {
                KeywordScores = batchResponses
                    .SelectMany(response => response.KeywordScores)
                    .ToList()
            };

            return _merger.Merge(keywords, combinedLlmResponse);
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

    private async Task<KeywordAnalysisResponse> ScoreKeywordBatchAsync(
        string aiModel,
        string apiKey,
        KeywordExtractionResponse keywords,
        string resumeText,
        int batchNumber,
        int totalBatches,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(keywords, resumeText, batchNumber, totalBatches);
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
            AtsContextualKeywordScoringSchema.Schema,
            _options.KeywordAnalysisMaxOutputTokens);
        var json = JsonSerializer.Serialize(requestBody);

        _logger.LogInformation(
            "Starting ATS contextual keyword scoring batch. Model: {Model}. BatchNumber: {BatchNumber}. TotalBatches: {TotalBatches}. KeywordCount: {KeywordCount}. ResumeTextLength: {ResumeTextLength}. MaxOutputTokens: {MaxOutputTokens}.",
            aiModel,
            batchNumber,
            totalBatches,
            keywords.Keywords.Count,
            resumeText.Length,
            _options.KeywordAnalysisMaxOutputTokens);

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
            $"ATS contextual keyword scoring batch {batchNumber}",
            cancellationToken);
        stopwatch.Stop();
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = OpenAiErrorInfo.Parse(responseText);
            _logger.LogError(
                "OpenAI ATS contextual keyword scoring batch failed. Model: {Model}. Status: {Status}. ElapsedMs: {ElapsedMs}. ResponseBodyLength: {ResponseBodyLength}. BatchNumber: {BatchNumber}. KeywordCount: {KeywordCount}. ErrorType: {ErrorType}. ErrorCode: {ErrorCode}. ErrorParam: {ErrorParam}. ErrorMessage: {ErrorMessage}.",
                aiModel,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                responseText.Length,
                batchNumber,
                keywords.Keywords.Count,
                error.Type,
                error.Code,
                error.Param,
                error.Message);

            throw new InvalidOperationException(
                $"OpenAI ATS contextual keyword scoring failed: {response.StatusCode}.");
        }

        var usage = OpenAiResponseUsageParser.Parse(responseText);
        var llmScores = OpenAiJsonResponseDeserializer.DeserializeStructuredOutput<KeywordAnalysisResponse>(
            responseText,
            SchemaName,
            aiModel,
            usage,
            _options.KeywordAnalysisMaxOutputTokens,
            batchNumber,
            keywords.Keywords.Count);

        var outputLength = OpenAiResponseParser.ExtractTextOutput(responseText).Length;
        _logger.LogInformation(
            "ATS contextual keyword scoring batch succeeded. Model: {Model}. BatchNumber: {BatchNumber}. TotalBatches: {TotalBatches}. ElapsedMs: {ElapsedMs}. ResponseBodyLength: {ResponseBodyLength}. OutputLength: {OutputLength}. ReturnedKeywordCount: {ReturnedKeywordCount}. InputTokens: {InputTokens}. OutputTokens: {OutputTokens}. TotalTokens: {TotalTokens}. CachedTokens: {CachedTokens}. MaxOutputTokens: {MaxOutputTokens}.",
            aiModel,
            batchNumber,
            totalBatches,
            stopwatch.ElapsedMilliseconds,
            responseText.Length,
            outputLength,
            llmScores.KeywordScores.Count,
            usage.InputTokens,
            usage.OutputTokens,
            usage.TotalTokens,
            usage.CachedTokens,
            _options.KeywordAnalysisMaxOutputTokens);

        return llmScores;
    }

    private int GetBatchSize()
    {
        return Math.Max(1, _options.KeywordAnalysisBatchSize);
    }

    private static string BuildPrompt(
        KeywordExtractionResponse keywords,
        string resumeText,
        int batchNumber,
        int totalBatches)
    {
        var keywordPayload = new
        {
            keywords = keywords.Keywords.Select(keyword => new
            {
                keyword = keyword.Keyword,
                variations = keyword.Variations
            })
        };
        var keywordsJson = JsonSerializer.Serialize(keywordPayload);

        return $$"""
You are an ATS and resume expert. For each keyword provided in this batch, scan the entire resume and determine how it is used contextually.

Batch: {{batchNumber}} of {{totalBatches}}.

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
- For each keyword that is present, return at most 1 evidence snippet.
- Each evidence snippet must include section, text, and matched_term.
- If the keyword is not present, evidence must be an empty array.
- Do not invent evidence. Use only resume text.

Output rules:
- Do not return a context field. The original job-description context will be copied into the final response by the API.

Return ONLY valid JSON with NO additional text or markdown.

JD Keywords:
{{keywordsJson}}

Resume:
{{resumeText}}
""";
    }
}
