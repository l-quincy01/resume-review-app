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

public class HeaderValidationValidationTests
{
    [Fact]
    public async Task ValidateHeaders_RejectsMissingResume()
    {
        var controller = CreateController();
        var request = new HeaderValidationRequest();

        var result = await controller.ValidateHeaders(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Resume PDF is required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ValidateHeaders_RejectsNonPdfUploads()
    {
        var controller = CreateController();
        var request = new HeaderValidationRequest
        {
            Resume = CreateFile("resume.txt", "text/plain", 32)
        };

        var result = await controller.ValidateHeaders(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Only PDF", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ValidateHeaders_RejectsOversizedPdfs()
    {
        var controller = CreateController(new OpenAiOptions
        {
            MaxResumeBytes = 10
        });
        var request = new HeaderValidationRequest
        {
            Resume = CreateFile("resume.pdf", "application/pdf", 32)
        };

        var result = await controller.ValidateHeaders(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Resume PDF must be", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task ValidateHeaders_ExtractsTextAndReturnsHeaderValidation()
    {
        var extractor = new StubResumeTextExtractor("""
Summary
Skills
Work Experience
Education
""");
        var controller = CreateController(extractor: extractor);
        var request = new HeaderValidationRequest
        {
            Resume = CreateFile("resume.pdf", "application/pdf", 32)
        };

        var result = await controller.ValidateHeaders(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HeaderValidationResponse>(ok.Value);
        Assert.Equal("strong", response.StructureQuality);
        Assert.True(extractor.WasCalled);
    }

    private static HeaderValidation CreateController(
        OpenAiOptions? options = null,
        IResumeTextExtractor? extractor = null,
        IStandardHeaderValidator? validator = null)
    {
        options ??= new OpenAiOptions
        {
            MaxResumeBytes = 1024
        };

        return new HeaderValidation(
            extractor ?? new StubResumeTextExtractor(""),
            validator ?? new StandardHeaderValidator(),
            Microsoft.Extensions.Options.Options.Create(options),
            new ListLogger<HeaderValidation>());
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
}
