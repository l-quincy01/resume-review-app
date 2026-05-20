using System.Text.Json;

namespace ResumeReview.Api.Services.Providers.OpenAI;

public sealed record OpenAiErrorInfo(
    string? Type,
    string? Code,
    string? Param,
    string? Message)
{
    public static OpenAiErrorInfo Parse(string responseJson)
    {
        try
        {
            using var document = JsonDocument.Parse(responseJson);
            if (!document.RootElement.TryGetProperty("error", out var error) ||
                error.ValueKind != JsonValueKind.Object)
            {
                return new OpenAiErrorInfo(null, null, null, null);
            }

            return new OpenAiErrorInfo(
                GetString(error, "type"),
                GetString(error, "code"),
                GetString(error, "param"),
                Truncate(GetString(error, "message"), 300));
        }
        catch (JsonException)
        {
            return new OpenAiErrorInfo(null, null, null, null);
        }
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
