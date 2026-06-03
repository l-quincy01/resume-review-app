namespace ResumeReview.Api.Services.OpenAiApiKeys;

public sealed class OpenAiApiKeyHeaderRedactionMiddleware
{
    private readonly RequestDelegate _next;

    public OpenAiApiKeyHeaderRedactionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = context.Request.Headers[OpenAiApiKeyProvider.HeaderName].ToString();

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            context.Items[OpenAiApiKeyProvider.HttpContextItemKey] = apiKey.Trim();
            context.Request.Headers[OpenAiApiKeyProvider.HeaderName] = OpenAiApiKeyProvider.RedactedValue;
        }

        await _next(context);
    }
}
