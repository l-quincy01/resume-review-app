using System.Text.Json;

namespace ResumeReview.Api.Services.Providers.OpenAI;

public sealed record OpenAiResponseUsage(
    int? InputTokens,
    int? OutputTokens,
    int? TotalTokens,
    int? CachedTokens);

public static class OpenAiResponseUsageParser
{
    public static OpenAiResponseUsage Parse(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);

        if (!document.RootElement.TryGetProperty("usage", out var usage) ||
            usage.ValueKind != JsonValueKind.Object)
        {
            return new OpenAiResponseUsage(null, null, null, null);
        }

        var inputTokens = GetInt(usage, "input_tokens") ?? GetInt(usage, "prompt_tokens");
        var outputTokens = GetInt(usage, "output_tokens") ?? GetInt(usage, "completion_tokens");
        var totalTokens = GetInt(usage, "total_tokens");
        int? cachedTokens = null;

        if (usage.TryGetProperty("input_tokens_details", out var inputDetails))
        {
            cachedTokens = GetInt(inputDetails, "cached_tokens");
        }
        else if (usage.TryGetProperty("prompt_tokens_details", out var promptDetails))
        {
            cachedTokens = GetInt(promptDetails, "cached_tokens");
        }

        return new OpenAiResponseUsage(inputTokens, outputTokens, totalTokens, cachedTokens);
    }

    private static int? GetInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : null;
    }
}
