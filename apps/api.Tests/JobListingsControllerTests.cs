using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    }

    private static JobListingsController CreateController(IJobListingsService? service = null)
    {
        return new JobListingsController(
            service ?? new StubJobListingsService(),
            new ListLogger<JobListingsController>());
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

        public Task<JobListings> FindJobListingsAsync(
            string apiKey,
            string aiModel,
            JobSearchProfile profile,
            CancellationToken cancellationToken = default)
        {
            LastApiKey = apiKey;

            return Task.FromResult(new JobListings());
        }
    }
}
