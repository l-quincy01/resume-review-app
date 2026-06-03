using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ResumeReview.Api.Controllers;
using ResumeReview.Api.Models;
using ResumeReview.Api.Services.JobSearchService.Listings;

namespace ResumeReview.Api.Tests;

public class JobListingsControllerTests
{
    [Fact]
    public async Task GetJobListings_RejectsMissingOpenAiApiKey()
    {
        var controller = CreateController();
        AddHttpContext(controller);

        var result = await controller.GetJobListings(
            new JobSearchProfile { Titles = ["Developer"] },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("OpenAI API key is required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task GetJobListings_PassesOpenAiApiKeyToService()
    {
        var service = new StubJobListingsService();
        var controller = CreateController(service);
        AddOpenAiApiKeyHeader(controller);

        var result = await controller.GetJobListings(
            new JobSearchProfile { Titles = ["Developer"] },
            CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("user-test-key", service.LastApiKey);
        Assert.Equal("gpt-5", service.LastAiModel);
    }

    [Fact]
    public async Task GetJobListings_LogsProfileValuesOutsideProduction()
    {
        var logger = new ListLogger<JobListingsController>();
        var controller = CreateController(logger: logger);
        AddOpenAiApiKeyHeader(controller);

        await controller.GetJobListings(
            new JobSearchProfile
            {
                Titles = ["Software Engineer", "API Developer"],
                Keywords = ["Java", "Spring Boot"],
                Seniority = "Entry level",
                Locations = ["Pretoria"],
                Exclude = ["Senior"]
            },
            CancellationToken.None);

        Assert.Contains(logger.Entries, entry =>
            entry.Message.Contains("Software Engineer") &&
            entry.Message.Contains("Spring Boot") &&
            entry.Message.Contains("Senior"));
    }

    [Fact]
    public async Task GetJobListings_DoesNotLogProfileValuesInProduction()
    {
        var logger = new ListLogger<JobListingsController>();
        var controller = CreateController(
            logger: logger,
            environmentName: Environments.Production);
        AddOpenAiApiKeyHeader(controller);

        await controller.GetJobListings(
            new JobSearchProfile
            {
                Titles = ["Sensitive Title"],
                Keywords = ["Sensitive Keyword"],
                Seniority = "Entry level",
                Locations = ["Sensitive Location"],
                Exclude = ["Sensitive Exclude"]
            },
            CancellationToken.None);

        Assert.DoesNotContain(logger.Entries, entry =>
            entry.Message.Contains("Sensitive Title") ||
            entry.Message.Contains("Sensitive Keyword") ||
            entry.Message.Contains("Sensitive Location") ||
            entry.Message.Contains("Sensitive Exclude"));
        Assert.Contains(logger.Entries, entry => entry.Message.Contains("ProfileHash"));
    }

    [Fact]
    public async Task GetJobListings_RejectsProfileWithoutTitlesOrKeywords()
    {
        var controller = CreateController();
        AddOpenAiApiKeyHeader(controller);

        var result = await controller.GetJobListings(
            new JobSearchProfile(),
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("At least one title or keyword is required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task GetJobListings_ReturnsBadGatewayWhenProviderFails()
    {
        var controller = CreateController(new StubJobListingsService
        {
            ThrowOnCall = true
        });
        AddOpenAiApiKeyHeader(controller);

        var result = await controller.GetJobListings(
            new JobSearchProfile { Titles = ["Developer"] },
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, objectResult.StatusCode);
        Assert.Contains("Job listings search failed", objectResult.Value!.ToString());
    }

    [Fact]
    public async Task GetJobListings_PreservesSuccessfulEmptyListings()
    {
        var controller = CreateController(new StubJobListingsService
        {
            Response = new JobListings { jobListings = [] }
        });
        AddOpenAiApiKeyHeader(controller);

        var result = await controller.GetJobListings(
            new JobSearchProfile { Titles = ["Developer"] },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<JobListings>(ok.Value);
        Assert.Empty(response.jobListings);
    }

    private static JobListingsController CreateController(
        IJobListingsService? service = null,
        ListLogger<JobListingsController>? logger = null,
        string environmentName = "Development")
    {
        return new JobListingsController(
            service ?? new StubJobListingsService(),
            new StubWebHostEnvironment { EnvironmentName = environmentName },
            logger ?? new ListLogger<JobListingsController>());
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

    private sealed class StubJobListingsService : IJobListingsService
    {
        public string? LastApiKey { get; private set; }
        public string? LastAiModel { get; private set; }
        public bool ThrowOnCall { get; init; }
        public JobListings Response { get; init; } = new();

        public Task<JobListings> FindJobListingsAsync(
            string apiKey,
            string aiModel,
            JobSearchProfile profile,
            CancellationToken cancellationToken = default)
        {
            LastApiKey = apiKey;
            LastAiModel = aiModel;

            if (ThrowOnCall)
            {
                throw new JobListingsProviderException("provider failed");
            }

            return Task.FromResult(Response);
        }
    }

    private sealed class StubWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "api.Tests";
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
