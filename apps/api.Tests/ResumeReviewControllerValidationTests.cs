using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Controllers;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services;

namespace ResumeReview.Api.Tests;

public class ResumeReviewControllerValidationTests
{
    [Fact]
    public async Task Submit_RejectsNonPdfUploads()
    {
        var controller = CreateController();
        var request = new ResumeReviewRequest
        {
            AiModel = "gpt-4.1-mini",
            Resume = CreateFile("resume.txt", "text/plain", 32)
        };

        var result = await controller.Submit(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Only PDF", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task Stream_RejectsNonPdfUploadsBeforeStreaming()
    {
        var controller = CreateController();
        var request = new ResumeReviewRequest
        {
            AiModel = "gpt-4.1-mini",
            Resume = CreateFile("resume.txt", "text/plain", 32)
        };

        var result = await controller.Stream(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Only PDF", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task Submit_RejectsOversizedPdfs()
    {
        var controller = CreateController(new OpenAiOptions
        {
            MaxResumeBytes = 10,
            AllowedModels = ["gpt-4.1-mini"]
        });
        var request = new ResumeReviewRequest
        {
            AiModel = "gpt-4.1-mini",
            Resume = CreateFile("resume.pdf", "application/pdf", 32)
        };

        var result = await controller.Submit(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Resume PDF must be", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task Submit_RejectsModelsOutsideAllowlist()
    {
        var controller = CreateController(new OpenAiOptions
        {
            MaxResumeBytes = 1024,
            AllowedModels = ["gpt-4.1-mini"]
        });
        var request = new ResumeReviewRequest
        {
            AiModel = "expensive-model",
            Resume = CreateFile("resume.pdf", "application/pdf", 32)
        };

        var result = await controller.Submit(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Selected AI model is not allowed", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task Submit_AcceptsMultipartPdfAndCallsService()
    {
        var service = new RecordingResumeReviewService();
        var controller = CreateController(service: service);
        var request = new ResumeReviewRequest
        {
            AiModel = "gpt-4.1-mini",
            JobDescription = "Build APIs",
            Resume = CreateFile("resume.pdf", "application/pdf", 32)
        };

        var result = await controller.Submit(request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.True(service.WasCalled);
        Assert.Equal("resume.pdf", service.LastRequest?.Resume.FileName);
        Assert.Equal("Build APIs", service.LastRequest?.JobDescription);
    }

    private static ResumeReviewController CreateController(
        OpenAiOptions? options = null,
        IResumeReviewService? service = null)
    {
        options ??= new OpenAiOptions
        {
            MaxResumeBytes = 1024,
            AllowedModels = ["gpt-4.1-mini"],
            MaxJobDescriptionCharacters = 100
        };

        return new ResumeReviewController(
            service ?? new RecordingResumeReviewService(),
            Microsoft.Extensions.Options.Options.Create(options));
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

    private sealed class RecordingResumeReviewService : IResumeReviewService
    {
        public bool WasCalled { get; private set; }
        public ResumeReviewRequest? LastRequest { get; private set; }

        public Task<ResumeReviewResponse> AnalyzeAsync(
            ResumeReviewRequest request,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            LastRequest = request;

            return Task.FromResult(new ResumeReviewResponse());
        }

        public async IAsyncEnumerable<ResumeReviewStreamEnvelope> AnalyzeStreamAsync(
            ResumeReviewRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            WasCalled = true;
            LastRequest = request;

            yield return new ResumeReviewStreamEnvelope
            {
                EventName = "review_completed",
                Data = new ResumeReviewStreamEvent
                {
                    Payload = new ResumeReviewResponse()
                }
            };

            await Task.CompletedTask;
        }
    }
}
