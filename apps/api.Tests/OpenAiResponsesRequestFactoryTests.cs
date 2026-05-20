using System.Text.Json;
using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Tests;

public class OpenAiResponsesRequestFactoryTests
{
    [Fact]
    public void CreateStructuredRequest_AddsLowVerbosity_MaxOutputTokens_AndGpt5MinimalReasoning()
    {
        var request = OpenAiResponsesRequestFactory.CreateStructuredRequest(
            "gpt-5-nano",
            [new { role = "user", content = new[] { new { type = "input_text", text = "redacted" } } }],
            "test_schema",
            new { type = "object", additionalProperties = false },
            maxOutputTokens: 1234);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(request));
        var root = document.RootElement;

        Assert.Equal("gpt-5-nano", root.GetProperty("model").GetString());
        Assert.False(root.TryGetProperty("temperature", out _));
        Assert.Equal(1234, root.GetProperty("max_output_tokens").GetInt32());
        Assert.Equal("low", root.GetProperty("text").GetProperty("verbosity").GetString());
        Assert.Equal("minimal", root.GetProperty("reasoning").GetProperty("effort").GetString());
        Assert.Equal("test_schema", root.GetProperty("text").GetProperty("format").GetProperty("name").GetString());
        Assert.True(root.GetProperty("text").GetProperty("format").GetProperty("strict").GetBoolean());
    }

    [Fact]
    public void CreateStructuredRequest_DoesNotAddReasoning_ForNonGpt5Models()
    {
        var request = OpenAiResponsesRequestFactory.CreateStructuredRequest(
            "gpt-4.1-mini",
            [new { role = "user", content = new[] { new { type = "input_text", text = "redacted" } } }],
            "test_schema",
            new { type = "object", additionalProperties = false });

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(request));

        Assert.False(document.RootElement.TryGetProperty("reasoning", out _));
        Assert.False(document.RootElement.TryGetProperty("max_output_tokens", out _));
        Assert.Equal(0, document.RootElement.GetProperty("temperature").GetInt32());
    }

    [Fact]
    public void CreateStructuredRequest_DoesNotAddTemperature_ForGpt5Mini()
    {
        var request = OpenAiResponsesRequestFactory.CreateStructuredRequest(
            "gpt-5-mini",
            [new { role = "user", content = new[] { new { type = "input_text", text = "redacted" } } }],
            "test_schema",
            new { type = "object", additionalProperties = false });

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(request));

        Assert.False(document.RootElement.TryGetProperty("temperature", out _));
    }
}
