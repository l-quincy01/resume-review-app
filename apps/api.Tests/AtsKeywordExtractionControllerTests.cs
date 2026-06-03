using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Controllers;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.AtsService.KeywordAnalysis;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Tests;

public class KeywordExtractionControllerTests
{
    [Fact]
    public async Task ExtractKeywords_RejectsMissingBody()
    {
        var controller = CreateController();

        var result = await controller.ExtractKeywords(null, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Request body is required", badRequest.Value!.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExtractKeywords_RejectsEmptyJobDescription(string jobDescription)
    {
        var controller = CreateController();

        var result = await controller.ExtractKeywords(
            new KeywordExtractionRequest
            {
                JobDescription = jobDescription,
                AiModel = "gpt-4.1-mini"
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Job description is required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ExtractKeywords_RejectsOversizedJobDescription()
    {
        var controller = CreateController(options: new OpenAiOptions
        {
            MaxJobDescriptionCharacters = 10,
            AllowedModels = ["gpt-4.1-mini"]
        });

        var result = await controller.ExtractKeywords(
            new KeywordExtractionRequest
            {
                JobDescription = new string('a', 11),
                AiModel = "gpt-4.1-mini"
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Job description must be", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ExtractKeywords_RejectsMissingModel()
    {
        var controller = CreateController();

        var result = await controller.ExtractKeywords(
            new KeywordExtractionRequest
            {
                JobDescription = "Build React apps.",
                AiModel = ""
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("AI model is required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ExtractKeywords_RejectsDisallowedModel()
    {
        var controller = CreateController();

        var result = await controller.ExtractKeywords(
            new KeywordExtractionRequest
            {
                JobDescription = "Build React apps.",
                AiModel = "expensive-model"
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Selected AI model is not allowed", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ExtractKeywords_ReturnsSuccessfulResponse()
    {
        var service = new StubKeywordExtractionService
        {
            Response = new KeywordExtractionResponse
            {
                JobTitle = "Frontend Developer",
                Keywords =
                [
                    new KeywordExtractionItemResponse
                    {
                        Keyword = "React",
                        KeywordType = "single_word",
                        Category = "framework",
                        Tier = 1,
                        Requirement = "must_have",
                        Context = "Used to build UI.",
                        Frequency = 1,
                        BoostApplied = false
                    }
                ]
            }
        };
        var controller = CreateController(service: service);
        AddOpenAiApiKeyHeader(controller);

        var result = await controller.ExtractKeywords(
            new KeywordExtractionRequest
            {
                JobDescription = "Build React apps.",
                AiModel = "gpt-4.1-mini"
            },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<KeywordExtractionResponse>(ok.Value);
        Assert.Equal("Frontend Developer", response.JobTitle);
        Assert.Equal("gpt-4.1-mini", service.LastModel);
        Assert.Equal("Build React apps.", service.LastJobDescription);
    }

    [Fact]
    public async Task ExtractKeywords_RejectsMissingOpenAiApiKey()
    {
        var controller = CreateController();
        AddHttpContext(controller);

        var result = await controller.ExtractKeywords(
            new KeywordExtractionRequest
            {
                JobDescription = "Build React apps.",
                AiModel = "gpt-4.1-mini"
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("OpenAI API key is required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ExtractKeywords_ServiceFailureReturnsBadGatewayWithoutLoggingRawJobDescription()
    {
        const string rawJobDescription = "SENSITIVE_JOB_DESCRIPTION";
        var logger = new ListLogger<KeywordExtractionController>();
        var controller = CreateController(
            service: new StubKeywordExtractionService { ThrowOnCall = true },
            logger: logger);
        AddOpenAiApiKeyHeader(controller);

        var result = await controller.ExtractKeywords(
            new KeywordExtractionRequest
            {
                JobDescription = rawJobDescription,
                AiModel = "gpt-4.1-mini"
            },
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, objectResult.StatusCode);
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains(rawJobDescription));
    }

    private static KeywordExtractionController CreateController(
        OpenAiOptions? options = null,
        IKeywordExtractionService? service = null,
        ILogger<KeywordExtractionController>? logger = null)
    {
        options ??= new OpenAiOptions
        {
            MaxJobDescriptionCharacters = 12000,
            AllowedModels = ["gpt-4.1-mini"]
        };

        return new KeywordExtractionController(
            service ?? new StubKeywordExtractionService(),
            Microsoft.Extensions.Options.Options.Create(options),
            logger ?? new ListLogger<KeywordExtractionController>());
    }

    private sealed class StubKeywordExtractionService : IKeywordExtractionService
    {
        public KeywordExtractionResponse Response { get; set; } = new();
        public bool ThrowOnCall { get; set; }
        public string? LastModel { get; private set; }
        public string? LastApiKey { get; private set; }
        public string? LastJobDescription { get; private set; }

        public Task<KeywordExtractionResponse> ExtractKeywordsAsync(
            string apiKey,
            string aiModel,
            string jobDescription,
            CancellationToken cancellationToken = default)
        {
            LastApiKey = apiKey;
            LastModel = aiModel;
            LastJobDescription = jobDescription;

            if (ThrowOnCall)
            {
                throw new InvalidOperationException("provider failed");
            }

            return Task.FromResult(Response);
        }
    }

    private static void AddOpenAiApiKeyHeader(ControllerBase controller)
    {
        AddHttpContext(controller);
        controller.Request.Headers["X-OpenAI-Api-Key"] = "user-test-key";
    }

    private static void AddHttpContext(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
}
