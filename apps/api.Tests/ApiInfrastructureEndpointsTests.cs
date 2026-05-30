using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ResumeReview.Api.Services.AbuseProtection;

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

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private static StringContent JsonContent(string json)
    {
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }
}
