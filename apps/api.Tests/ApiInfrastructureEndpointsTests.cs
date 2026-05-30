using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ResumeReview.Api.Services.AbuseProtection;
using ResumeReview.Api.Services.OpenAiApiKeys;

namespace ResumeReview.Api.Tests;

public sealed class ApiInfrastructureEndpointsTests
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoint_ReturnsOk(string path)
    {
        using var factory = CreateFactory(new Dictionary<string, string?>
        {
            ["AbuseProtection:GlobalPermitLimitPerMinute"] = "1"
        });
        using var client = CreateClient(factory);

        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/resume-review")]
    [InlineData("/api/v1/resume-review")]
    public async Task ResumeReviewRoute_RejectsUnsupportedGet_ForUnversionedAndV1Routes(string path)
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task ExpensiveAiRoutes_ReturnTooManyRequests_WhenPolicyLimitIsExceeded()
    {
        using var factory = CreateFactory(new Dictionary<string, string?>
        {
            ["AbuseProtection:GlobalPermitLimitPerMinute"] = "100",
            ["AbuseProtection:ExpensiveAiPermitLimitPerTenMinutes"] = "1"
        });
        using var client = CreateClient(factory);

        using var firstResponse = await client.PostAsync(
            "/api/ats-engine/keyword-extraction",
            JsonContent("{}"));
        using var secondResponse = await client.PostAsync(
            "/api/v1/ats-engine/keyword-extraction",
            JsonContent("{}"));

        Assert.NotEqual(HttpStatusCode.TooManyRequests, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
    }

    [Fact]
    public async Task LocalAtsRoutes_ReturnTooManyRequests_WhenPolicyLimitIsExceeded()
    {
        using var factory = CreateFactory(new Dictionary<string, string?>
        {
            ["AbuseProtection:GlobalPermitLimitPerMinute"] = "100",
            ["AbuseProtection:LocalAtsPermitLimitPerMinute"] = "1"
        });
        using var client = CreateClient(factory);

        using var firstResponse = await client.PostAsync(
            "/api/ats-engine/keyword-scoring",
            JsonContent("{}"));
        using var secondResponse = await client.PostAsync(
            "/api/v1/ats-engine/keyword-scoring",
            JsonContent("{}"));

        Assert.NotEqual(HttpStatusCode.TooManyRequests, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
    }

    [Fact]
    public async Task OversizedJsonBody_ReturnsPayloadTooLarge()
    {
        using var factory = CreateFactory(new Dictionary<string, string?>
        {
            ["AbuseProtection:GlobalPermitLimitPerMinute"] = "100",
            ["AbuseProtection:MaxJsonBodyBytes"] = "16"
        });
        using var client = CreateClient(factory);

        using var response = await client.PostAsync(
            "/api/ats-engine/keyword-scoring",
            JsonContent("""{"keyword_scores":"SENSITIVE_RAW_BODY"}"""));

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public void AbuseProtectionLogger_LogsSafeRateLimitMetadata()
    {
        var listLogger = new ListLogger<AbuseProtectionLogger>();
        var logger = new AbuseProtectionLogger(listLogger);

        logger.LogRateLimitRejected(
            AbuseProtectionPolicyNames.ExpensiveAi,
            "/api/ats-engine/keyword-extraction",
            "POST",
            "abc123",
            1,
            600,
            42);

        var entry = Assert.Single(listLogger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("expensive-ai", entry.Message);
        Assert.Contains("/api/ats-engine/keyword-extraction", entry.Message);
        Assert.Contains("abc123", entry.Message);
        Assert.DoesNotContain("SENSITIVE_JOB_DESCRIPTION", entry.Message);
    }

    [Fact]
    public void AbuseProtectionLogger_LogsSafeRequestBodyMetadata()
    {
        var listLogger = new ListLogger<AbuseProtectionLogger>();
        var logger = new AbuseProtectionLogger(listLogger);

        logger.LogRequestBodyRejected(
            "/api/ats-engine/keyword-scoring",
            "POST",
            "application/json",
            16,
            128);

        var entry = Assert.Single(listLogger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("/api/ats-engine/keyword-scoring", entry.Message);
        Assert.Contains("application/json", entry.Message);
        Assert.Contains("16", entry.Message);
        Assert.Contains("128", entry.Message);
        Assert.DoesNotContain("SENSITIVE_RAW_BODY", entry.Message);
    }

    [Fact]
    public async Task CorsPreflight_AllowsConfiguredFrontendAndOpenAiApiKeyHeader()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/resume-review");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", ["content-type", "x-openai-api-key"]);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            "http://localhost:3000",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Contains(
            OpenAiApiKeyProvider.HeaderName,
            response.Headers.GetValues("Access-Control-Allow-Headers").Single(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Production_ForwardedHttpsRequestReturnsOkWithHsts()
    {
        using var factory = CreateProductionFactory();
        using var client = CreateClient(factory, new Uri("https://api.example"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Forwarded-Proto", "https");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Strict-Transport-Security", response.Headers.Select(header => header.Key));
    }

    [Fact]
    public async Task DockerLocal_AllowsLocalhostHttpCorsOrigin()
    {
        using var factory = CreateDockerLocalFactory();
        using var client = CreateClient(factory);
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/resume-review");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", ["content-type", "x-openai-api-key"]);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            "http://localhost:3000",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public void Production_RejectsHttpCorsOrigin()
    {
        using var factory = CreateProductionFactory(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000"
        });

        var exception = Assert.Throws<InvalidOperationException>(() => CreateClient(factory));

        Assert.Contains("entry point exited", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenAiApiKeyHeaderRedactionMiddleware_RedactsHeaderForDownstreamReaders()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[OpenAiApiKeyProvider.HeaderName] = "user-secret-key";
        var middleware = new OpenAiApiKeyHeaderRedactionMiddleware(next =>
        {
            Assert.Equal(OpenAiApiKeyProvider.RedactedValue, next.Request.Headers[OpenAiApiKeyProvider.HeaderName]);
            Assert.Equal("user-secret-key", next.Items[OpenAiApiKeyProvider.HttpContextItemKey]);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);
    }

    private static WebApplicationFactory<Program> CreateFactory(
        Dictionary<string, string?>? configuration = null)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");

                if (configuration is not null)
                {
                    builder.ConfigureAppConfiguration((_, configBuilder) =>
                    {
                        configBuilder.AddInMemoryCollection(configuration);
                    });
                }
            });
    }

    private static WebApplicationFactory<Program> CreateProductionFactory()
    {
        return CreateProductionFactory(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://frontend.example",
            ["AllowedHosts"] = "api.example"
        });
    }

    private static WebApplicationFactory<Program> CreateProductionFactory(
        Dictionary<string, string?> configuration)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                foreach (var setting in configuration)
                {
                    builder.UseSetting(setting.Key, setting.Value);
                }

                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(configuration);
                });
            });
    }

    private static WebApplicationFactory<Program> CreateDockerLocalFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("DockerLocal");
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
                        ["AllowedHosts"] = "*"
                    });
                });
            });
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory, Uri? baseAddress = null)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = baseAddress ?? new Uri("http://localhost"),
            AllowAutoRedirect = false
        });
    }

    private static StringContent JsonContent(string json)
    {
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }
}
