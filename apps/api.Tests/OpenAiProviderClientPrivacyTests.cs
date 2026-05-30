using System.Net;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.Ai.Providers.OpenAi;
using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Tests;

public class OpenAiProviderClientPrivacyTests
{
    [Fact]
    public async Task UploadFileAsync_DoesNotLogOrThrowRawProviderResponseBody()
    {
        const string sensitiveBody = "SENSITIVE_RESUME_TEXT_FROM_PROVIDER";
        var logger = new ListLogger<OpenAiProviderClient>();
        var client = CreateClient(logger, new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(sensitiveBody)
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.UploadFileAsync(
                "user-test-key",
                new MemoryStream([1, 2, 3]),
                "resume.pdf",
                "application/pdf",
                CancellationToken.None));

        Assert.DoesNotContain(sensitiveBody, exception.Message);
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains(sensitiveBody));
        Assert.Contains(logger.Entries, entry => entry.Message.Contains("ResponseBodyLength"));
    }

    [Fact]
    public async Task SendStructuredRequestAsync_DoesNotLogOrThrowRawProviderResponseBody()
    {
        const string sensitiveBody = "SENSITIVE_AI_RESPONSE_BODY";
        var logger = new ListLogger<OpenAiProviderClient>();
        var client = CreateClient(logger, new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(sensitiveBody)
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.SendStructuredRequestAsync<object>(
                "user-test-key",
                "gpt-4.1-mini",
                "file-test",
                "prompt",
                "schema",
                new { type = "object" },
                CancellationToken.None));

        Assert.DoesNotContain(sensitiveBody, exception.Message);
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains(sensitiveBody));
        Assert.Contains(logger.Entries, entry => entry.Message.Contains("ResponseBodyLength"));
    }

    [Fact]
    public async Task SendStructuredRequestAsync_UsesProvidedApiKeyForAuthorizationHeader()
    {
        var responseBody = """
        {
          "output": [
            {
              "type": "message",
              "content": [
                { "type": "output_text", "text": "{}" }
              ]
            }
          ]
        }
        """;
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody)
        });
        var client = CreateClient(new ListLogger<OpenAiProviderClient>(), handler);

        await client.SendStructuredRequestAsync<object>(
            "user-test-key",
            "gpt-4.1-mini",
            "file-test",
            "prompt",
            "schema",
            new { type = "object" },
            CancellationToken.None);

        Assert.Equal("Bearer", handler.LastAuthorizationScheme);
        Assert.Equal("user-test-key", handler.LastAuthorizationParameter);
    }

    private static OpenAiProviderClient CreateClient(
        ILogger<OpenAiProviderClient> logger,
        HttpResponseMessage response)
    {
        return CreateClient(logger, new StubHttpMessageHandler(response));
    }

    private static OpenAiProviderClient CreateClient(
        ILogger<OpenAiProviderClient> logger,
        HttpMessageHandler handler)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new OpenAiOptions
        {
            ApiKey = "test-key",
            MaxRetries = 0,
            AllowedModels = ["gpt-4.1-mini"]
        });
        var retryPolicy = new OpenAiRetryPolicy(
            options,
            new ListLogger<OpenAiRetryPolicy>());

        return new OpenAiProviderClient(
            new HttpClient(handler),
            options,
            retryPolicy,
            logger);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public string? LastAuthorizationScheme { get; private set; }
        public string? LastAuthorizationParameter { get; private set; }

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastAuthorizationScheme = request.Headers.Authorization?.Scheme;
            LastAuthorizationParameter = request.Headers.Authorization?.Parameter;

            return Task.FromResult(_response);
        }
    }
}
