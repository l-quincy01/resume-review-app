using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Models;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.JobSearchService.Listings;
using ResumeReview.Api.Services.JobSearchService.Providers.OpenAI;
using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Tests;

public class OpenAiJobListingsServiceTests
{
    [Fact]
    public async Task FindJobListingsAsync_SendsProvidedApiKeyAndParsesListings()
    {
        var handler = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(CreateOpenAiResponse("""
            {"jobListings":[{"listingTitle":"Backend Developer","companyName":"Acme","locationName":"Remote","whyItsGreat":"Matches backend API experience.","listingLink":"https://example.com/job","searchQuery":"Backend Developer Remote"}]}
            """))
        });
        var service = CreateService(handler);

        var result = await service.FindJobListingsAsync(
            "user-test-key",
            "gpt-5",
            new JobSearchProfile { Titles = ["Backend Developer"], Keywords = ["API"] },
            CancellationToken.None);

        Assert.Equal("Bearer", handler.LastAuthorizationScheme);
        Assert.Equal("user-test-key", handler.LastAuthorizationParameter);
        Assert.Single(result.jobListings);
        Assert.Equal("Backend Developer", result.jobListings.Single().ListingTitle);
    }

    [Fact]
    public async Task FindJobListingsAsync_UsesLowReasoningForGpt5WebSearch()
    {
        var handler = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(CreateOpenAiResponse("""{"jobListings":[]}"""))
        });
        var service = CreateService(handler);

        await service.FindJobListingsAsync(
            "user-test-key",
            "gpt-5",
            new JobSearchProfile { Titles = ["Backend Developer"], Keywords = ["API"] },
            CancellationToken.None);

        using var document = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("gpt-5", document.RootElement.GetProperty("model").GetString());
        Assert.Equal("web_search", document.RootElement.GetProperty("tools")[0].GetProperty("type").GetString());
        Assert.Equal("required", document.RootElement.GetProperty("tool_choice").GetString());
        Assert.Equal("low", document.RootElement.GetProperty("reasoning").GetProperty("effort").GetString());
    }

    [Fact]
    public async Task FindJobListingsAsync_UsesModelPassedToService()
    {
        var handler = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(CreateOpenAiResponse("""{"jobListings":[]}"""))
        });
        var service = CreateService(handler);

        await service.FindJobListingsAsync(
            "user-test-key",
            "gpt-5",
            new JobSearchProfile { Titles = ["Backend Developer"], Keywords = ["API"] },
            CancellationToken.None);

        using var document = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("gpt-5", document.RootElement.GetProperty("model").GetString());
    }

    [Fact]
    public async Task FindJobListingsAsync_UsesExactProfileValuesInPromptWithoutMutation()
    {
        var handler = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(CreateOpenAiResponse("""{"jobListings":[]}"""))
        });
        var service = CreateService(handler);

        await service.FindJobListingsAsync(
            "user-test-key",
            "gpt-5",
            new JobSearchProfile
            {
                Titles = ["Software Engineer", "API Developer"],
                Keywords = ["Java", "OpenAI API"],
                Seniority = "Entry level / Intern",
                Locations = ["Pretoria", "Grahamstown"],
                Exclude = ["Internship", "Junior", "Senior"]
            },
            CancellationToken.None);

        var prompt = ExtractPrompt(handler.LastRequestBody!);
        Assert.Contains("- Target titles: Software Engineer, API Developer", prompt);
        Assert.Contains("- Core keywords: Java, OpenAI API", prompt);
        Assert.Contains("- Seniority: Entry level / Intern", prompt);
        Assert.Contains("- Preferred locations: Pretoria, Grahamstown", prompt);
        Assert.Contains("- Exclude terms: Internship, Junior, Senior", prompt);
        Assert.DoesNotContain("Remote South Africa", prompt);
    }

    [Fact]
    public async Task FindJobListingsAsync_ThrowsOnProviderFailureWithoutLoggingRawProviderBodyOrApiKey()
    {
        const string sensitiveBody = "SENSITIVE_PROVIDER_BODY";
        const string apiKey = "user-secret-key";
        var logger = new ListLogger<OpenAiJobListingsService>();
        var service = CreateService(
            new RecordingHandler(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(sensitiveBody)
            }),
            logger);

        var exception = await Assert.ThrowsAsync<JobListingsProviderException>(() =>
            service.FindJobListingsAsync(
                apiKey,
                "gpt-5",
                new JobSearchProfile { Titles = ["Backend Developer"], Keywords = ["API"] },
                CancellationToken.None));

        Assert.DoesNotContain(sensitiveBody, exception.Message);
        Assert.DoesNotContain(apiKey, exception.Message);
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains(sensitiveBody));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains(apiKey));
        Assert.Contains(logger.Entries, entry => entry.Message.Contains("ResponseBodyLength"));
    }

    [Fact]
    public async Task FindJobListingsAsync_PreservesSuccessfulEmptyListings()
    {
        var service = CreateService(new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(CreateOpenAiResponse("""{"jobListings":[]}"""))
        }));

        var result = await service.FindJobListingsAsync(
            "user-test-key",
            "gpt-5",
            new JobSearchProfile { Titles = ["Backend Developer"] },
            CancellationToken.None);

        Assert.Empty(result.jobListings);
    }

    private static OpenAiJobListingsService CreateService(
        HttpMessageHandler handler,
        ILogger<OpenAiJobListingsService>? logger = null)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new OpenAiOptions
        {
            JobListingsModel = "gpt-5-nano",
            MaxJobListings = 5,
            MaxRetries = 0
        });

        return new OpenAiJobListingsService(
            new HttpClient(handler),
            options,
            new OpenAiRetryPolicy(options, new ListLogger<OpenAiRetryPolicy>()),
            logger ?? new ListLogger<OpenAiJobListingsService>());
    }

    private static string CreateOpenAiResponse(string outputText)
    {
        return $$"""
        {
          "output": [
            {
              "type": "message",
              "content": [
                { "type": "output_text", "text": {{JsonSerializer.Serialize(outputText)}} }
              ]
            }
          ]
        }
        """;
    }

    private static string ExtractPrompt(string requestBody)
    {
        using var document = JsonDocument.Parse(requestBody);
        return document.RootElement
            .GetProperty("input")[0]
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString()!;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public string? LastAuthorizationScheme { get; private set; }
        public string? LastAuthorizationParameter { get; private set; }
        public string? LastRequestBody { get; private set; }

        public RecordingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastAuthorizationScheme = request.Headers.Authorization?.Scheme;
            LastAuthorizationParameter = request.Headers.Authorization?.Parameter;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return _response;
        }
    }
}
