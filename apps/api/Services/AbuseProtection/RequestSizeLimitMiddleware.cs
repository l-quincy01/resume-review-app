using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Features;
using ResumeReview.Api.Options;

namespace ResumeReview.Api.Services.AbuseProtection;

public sealed class RequestSizeLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AbuseProtectionOptions _options;
    private readonly AbuseProtectionLogger _logger;

    public RequestSizeLimitMiddleware(
        RequestDelegate next,
        IOptions<AbuseProtectionOptions> options,
        AbuseProtectionLogger logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var maxBytes = GetMaxBodyBytes(context);
        if (maxBytes is null)
        {
            await _next(context);
            return;
        }

        var maxRequestBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (maxRequestBodySizeFeature is { IsReadOnly: false })
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = maxBytes.Value;
        }

        var contentLength = context.Request.ContentLength;
        if (contentLength.HasValue && contentLength.Value > maxBytes.Value)
        {
            _logger.LogRequestBodyRejected(
                context.Request.Path.Value,
                context.Request.Method,
                context.Request.ContentType,
                maxBytes.Value,
                contentLength.Value);

            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await context.Response.WriteAsJsonAsync(new { message = "Request body is too large." });
            return;
        }

        await _next(context);
    }

    private long? GetMaxBodyBytes(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(context.Request.ContentType))
        {
            return null;
        }

        if (context.Request.HasFormContentType)
        {
            return _options.MaxMultipartBodyBytes;
        }

        if (context.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return _options.MaxJsonBodyBytes;
        }

        return null;
    }
}
