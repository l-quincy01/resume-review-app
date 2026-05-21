using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AtsService.KeywordAnalysis;
using ResumeReview.Api.Services.AtsService.Providers.OpenAI;
using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Tests;

public class OpenAiKeywordAnalysisServiceTests
{
    [Fact]
    public async Task ScoreKeywordsAsync_SplitsKeywordsIntoConfiguredBatches()
    {
        var handler = new KeywordAnalysisHandler();
        var service = CreateService(handler, batchSize: 10);

        var result = await service.ScoreKeywordsAsync(
            "gpt-5-mini",
            CreateKeywords(40),
            "Work Experience\nBuilt Keyword 0 systems.",
            CancellationToken.None);

        Assert.Equal(4, handler.RequestBodies.Count);
        Assert.Equal(40, result.KeywordScores.Count);
        Assert.All(result.KeywordScores, score => Assert.True(score.Present));
    }

    [Fact]
    public async Task ScoreKeywordsAsync_PreservesSuccessfulBatchesWhenOneBatchReturnsMalformedJson()
    {
        var handler = new KeywordAnalysisHandler { MalformedBatchNumber = 2 };
        var logger = new ListLogger<OpenAiKeywordAnalysisService>();
        var service = CreateService(handler, batchSize: 10, logger: logger);

        var result = await service.ScoreKeywordsAsync(
            "gpt-5-mini",
            CreateKeywords(25),
            "Work Experience\nBuilt Keyword 0 systems.",
            CancellationToken.None);

        Assert.Equal(3, handler.RequestBodies.Count);
        Assert.Equal(25, result.KeywordScores.Count);
        Assert.Equal(15, result.KeywordScores.Count(score => score.Present));
        Assert.Equal(10, result.KeywordScores.Count(score => !score.Present));
        Assert.Contains(logger.Entries, entry =>
            entry.Level == LogLevel.Warning &&
            entry.Message.Contains("malformed JSON") &&
            !entry.Message.Contains("Work Experience"));
    }

    [Fact]
    public void SchemaName_RemainsStable()
    {
        Assert.Equal("ats_contextual_keyword_scoring", OpenAiKeywordAnalysisService.SchemaName);
    }

    private static OpenAiKeywordAnalysisService CreateService(
        HttpMessageHandler handler,
        int batchSize,
        ILogger<OpenAiKeywordAnalysisService>? logger = null)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new OpenAiOptions
        {
            ApiKey = "test-key",
            MaxRetries = 0,
            KeywordAnalysisBatchSize = batchSize,
            KeywordAnalysisMaxOutputTokens = 8000
        });

        return new OpenAiKeywordAnalysisService(
            new HttpClient(handler),
            options,
            new OpenAiRetryPolicy(options, new ListLogger<OpenAiRetryPolicy>()),
            new KeywordAnalysisMerger(),
            logger ?? new ListLogger<OpenAiKeywordAnalysisService>());
    }

    private static KeywordExtractionResponse CreateKeywords(int count)
    {
        return new KeywordExtractionResponse
        {
            JobTitle = "Developer",
            Keywords = Enumerable.Range(0, count)
                .Select(index => new KeywordExtractionItemResponse
                {
                    Keyword = $"Keyword {index}",
                    KeywordType = "multi_word",
                    Category = "technical_skill",
                    Tier = 1,
                    Requirement = "must_have",
                    Context = $"Context for keyword {index}.",
                    Variations = [],
                    Frequency = 1
                })
                .ToList()
        };
    }

    private sealed class KeywordAnalysisHandler : HttpMessageHandler
    {
        private static readonly Regex KeywordRegex = new("Keyword \\d+", RegexOptions.Compiled);

        public List<string> RequestBodies { get; } = [];
        public int? MalformedBatchNumber { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            RequestBodies.Add(body);

            var batchNumber = RequestBodies.Count;
            var outputText = MalformedBatchNumber == batchNumber
                ? """{"keyword_scores":["""
                : CreateModelOutput(body);

            var envelope = CreateResponseEnvelope(outputText);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(envelope, Encoding.UTF8, "application/json")
            };
        }

        private static string CreateModelOutput(string requestBody)
        {
            var keywords = KeywordRegex
                .Matches(requestBody)
                .Select(match => match.Value)
                .Distinct()
                .ToList();

            return JsonSerializer.Serialize(new
            {
                keyword_scores = keywords.Select(keyword => new
                {
                    keyword,
                    present = true,
                    matched_terms = new[] { keyword },
                    context_type = new
                    {
                        has_achievement = true,
                        has_metric = false,
                        has_action_verb = true,
                        in_experience_section = true,
                        in_project_section = false,
                        in_summary_section = false
                    },
                    evidence = new[]
                    {
                        new
                        {
                            section = "Work Experience",
                            text = $"Built {keyword} systems.",
                            matched_term = keyword
                        }
                    }
                })
            });
        }

        private static string CreateResponseEnvelope(string outputText)
        {
            return JsonSerializer.Serialize(new
            {
                output = new[]
                {
                    new
                    {
                        type = "message",
                        content = new[]
                        {
                            new
                            {
                                type = "output_text",
                                text = outputText
                            }
                        }
                    }
                },
                usage = new
                {
                    input_tokens = 10,
                    output_tokens = 20,
                    total_tokens = 30,
                    input_tokens_details = new
                    {
                        cached_tokens = 0
                    }
                }
            });
        }
    }
}
