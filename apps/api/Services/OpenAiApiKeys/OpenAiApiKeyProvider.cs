using Microsoft.AspNetCore.Mvc;

namespace ResumeReview.Api.Services.OpenAiApiKeys;

public static class OpenAiApiKeyProvider
{
    public const string HeaderName = "X-OpenAI-Api-Key";
    public const string RedactedValue = "[REDACTED]";
    public const string HttpContextItemKey = "OpenAiApiKey";
    private const int MaxApiKeyLength = 512;

    public static bool TryGetApiKey(
        ControllerBase controller,
        out string apiKey,
        out IActionResult? errorResult)
    {
        apiKey = controller.HttpContext.Items.TryGetValue(HttpContextItemKey, out var itemValue)
            ? itemValue?.ToString()?.Trim() ?? string.Empty
            : controller.Request.Headers[HeaderName].ToString().Trim();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            errorResult = controller.BadRequest(new { message = "OpenAI API key is required." });
            return false;
        }

        if (apiKey.Length > MaxApiKeyLength)
        {
            errorResult = controller.BadRequest(new { message = "OpenAI API key is invalid." });
            return false;
        }

        errorResult = null;
        return true;
    }
}
