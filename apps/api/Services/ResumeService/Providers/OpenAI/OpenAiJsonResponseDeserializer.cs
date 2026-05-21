using System.Text.Json;

namespace ResumeReview.Api.Services.Providers.OpenAI;

public static class OpenAiJsonResponseDeserializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static T DeserializeStructuredOutput<T>(
        string responseJson,
        string schemaName,
        string model,
        OpenAiResponseUsage usage,
        int maxOutputTokens,
        int? batchNumber,
        int keywordCount)
    {
        var outputText = OpenAiResponseParser.ExtractTextOutput(responseJson);

        try
        {
            using var _ = JsonDocument.Parse(outputText);
            return JsonSerializer.Deserialize<T>(outputText, SerializerOptions)
                ?? throw new JsonException("Structured output deserialized to null.");
        }
        catch (JsonException ex)
        {
            throw new OpenAiStructuredOutputException(
                schemaName,
                model,
                responseJson.Length,
                outputText.Length,
                batchNumber,
                keywordCount,
                ex.Path,
                ex.LineNumber,
                ex.BytePositionInLine,
                IsLikelyTruncated(usage, maxOutputTokens),
                ex);
        }
    }

    private static bool IsLikelyTruncated(OpenAiResponseUsage usage, int maxOutputTokens)
    {
        if (maxOutputTokens <= 0 || usage.OutputTokens is null)
        {
            return false;
        }

        return usage.OutputTokens.Value >= Math.Round(maxOutputTokens * 0.95m);
    }
}
