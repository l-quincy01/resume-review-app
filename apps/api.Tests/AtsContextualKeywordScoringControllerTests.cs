using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResumeReview.Api.Controllers;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AtsEngine;

namespace ResumeReview.Api.Tests;

public class AtsContextualKeywordScoringControllerTests
{
    [Fact]
    public async Task ScoreKeywords_RejectsMissingResume()
    {
        var controller = CreateController();
        var request = new ContextualKeywordScoringRequest
        {
            KeywordsJson = CreateKeywordsJson(),
            AiModel = "gpt-4.1-mini"
        };

        var result = await controller.ScoreKeywords(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Resume PDF is required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ScoreKeywords_RejectsNonPdfUploads()
    {
        var controller = CreateController();
        var request = CreateRequest(resume: CreateFile("resume.txt", "text/plain", 32));

        var result = await controller.ScoreKeywords(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Only PDF", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ScoreKeywords_RejectsOversizedPdfs()
    {
        var controller = CreateController(options: new OpenAiOptions
        {
            MaxResumeBytes = 10
        });
        var request = CreateRequest(resume: CreateFile("resume.pdf", "application/pdf", 32));

        var result = await controller.ScoreKeywords(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Resume PDF must be", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ScoreKeywords_RejectsMissingKeywordsJson()
    {
        var controller = CreateController();
        var request = CreateRequest(keywordsJson: "");

        var result = await controller.ScoreKeywords(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("keywords_json is required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ScoreKeywords_RejectsInvalidKeywordsJson()
    {
        var controller = CreateController();
        var request = CreateRequest(keywordsJson: "{not-json");

        var result = await controller.ScoreKeywords(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("keywords_json must be valid", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ScoreKeywords_RejectsKeywordsJsonWithoutKeywords()
    {
        var controller = CreateController();
        var request = CreateRequest(keywordsJson: JsonSerializer.Serialize(new KeywordExtractionResponse()));

        var result = await controller.ScoreKeywords(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("at least one keyword", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ScoreKeywords_RejectsMissingModel()
    {
        var controller = CreateController();
        var request = CreateRequest(aiModel: "");

        var result = await controller.ScoreKeywords(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("ai_model is required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ScoreKeywords_ReturnsSuccessfulResponse()
    {
        var extractor = new StubResumeTextExtractor("Built React dashboards.");
        var service = new StubContextualKeywordScoringService
        {
            Response = new ContextualKeywordScoringResponse
            {
                KeywordScores =
                [
                    new ContextualKeywordScoreResponse
                    {
                        Keyword = "React",
                        Present = true
                    }
                ]
            }
        };
        var controller = CreateController(extractor: extractor, service: service);
        var request = CreateRequest();

        var result = await controller.ScoreKeywords(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ContextualKeywordScoringResponse>(ok.Value);
        Assert.True(response.KeywordScores.Single().Present);
        Assert.True(extractor.WasCalled);
        Assert.Equal("gpt-4.1-mini", service.LastModel);
        Assert.Equal("Built React dashboards.", service.LastResumeText);
        Assert.Equal("React", service.LastKeywords?.Keywords.Single().Keyword);
    }

    [Fact]
    public async Task ScoreKeywords_ServiceFailureReturnsBadGatewayWithoutLoggingRawInputs()
    {
        const string resumeText = "SENSITIVE_RESUME_TEXT";
        var keywordsJson = CreateKeywordsJson("SENSITIVE_KEYWORD");
        var logger = new ListLogger<AtsContextualKeywordScoringController>();
        var controller = CreateController(
            extractor: new StubResumeTextExtractor(resumeText),
            service: new StubContextualKeywordScoringService { ThrowOnCall = true },
            logger: logger);
        var request = CreateRequest(keywordsJson: keywordsJson);

        var result = await controller.ScoreKeywords(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, objectResult.StatusCode);
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains(resumeText));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("SENSITIVE_KEYWORD"));
    }

    private static AtsContextualKeywordScoringController CreateController(
        OpenAiOptions? options = null,
        IResumeTextExtractor? extractor = null,
        IContextualKeywordScoringService? service = null,
        ILogger<AtsContextualKeywordScoringController>? logger = null)
    {
        options ??= new OpenAiOptions
        {
            MaxResumeBytes = 1024
        };

        return new AtsContextualKeywordScoringController(
            extractor ?? new StubResumeTextExtractor(""),
            service ?? new StubContextualKeywordScoringService(),
            Microsoft.Extensions.Options.Options.Create(options),
            logger ?? new ListLogger<AtsContextualKeywordScoringController>());
    }

    private static ContextualKeywordScoringRequest CreateRequest(
        IFormFile? resume = null,
        string? keywordsJson = null,
        string aiModel = "gpt-4.1-mini")
    {
        return new ContextualKeywordScoringRequest
        {
            Resume = resume ?? CreateFile("resume.pdf", "application/pdf", 32),
            KeywordsJson = keywordsJson ?? CreateKeywordsJson(),
            AiModel = aiModel
        };
    }

    private static string CreateKeywordsJson(string keyword = "React")
    {
        return JsonSerializer.Serialize(new KeywordExtractionResponse
        {
            JobTitle = "Frontend Developer",
            Keywords =
            [
                new KeywordExtractionItemResponse
                {
                    Keyword = keyword,
                    KeywordType = "single_word",
                    Category = "framework",
                    Tier = 1,
                    Requirement = "must_have",
                    Context = "Used to build UI.",
                    Frequency = 1
                }
            ]
        });
    }

    private static IFormFile CreateFile(string fileName, string contentType, int byteCount)
    {
        var bytes = Enumerable.Repeat((byte)'a', byteCount).ToArray();
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, stream.Length, "resume", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private sealed class StubResumeTextExtractor : IResumeTextExtractor
    {
        private readonly string _text;

        public StubResumeTextExtractor(string text)
        {
            _text = text;
        }

        public bool WasCalled { get; private set; }

        public Task<string> ExtractTextAsync(
            Stream pdfStream,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(_text);
        }
    }

    private sealed class StubContextualKeywordScoringService : IContextualKeywordScoringService
    {
        public ContextualKeywordScoringResponse Response { get; set; } = new();
        public bool ThrowOnCall { get; set; }
        public string? LastModel { get; private set; }
        public KeywordExtractionResponse? LastKeywords { get; private set; }
        public string? LastResumeText { get; private set; }

        public Task<ContextualKeywordScoringResponse> ScoreKeywordsAsync(
            string aiModel,
            KeywordExtractionResponse keywords,
            string resumeText,
            CancellationToken cancellationToken = default)
        {
            LastModel = aiModel;
            LastKeywords = keywords;
            LastResumeText = resumeText;

            if (ThrowOnCall)
            {
                throw new InvalidOperationException("provider failed");
            }

            return Task.FromResult(Response);
        }
    }
}
