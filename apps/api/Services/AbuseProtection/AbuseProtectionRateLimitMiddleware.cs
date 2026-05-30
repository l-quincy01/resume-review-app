using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Options;

namespace ResumeReview.Api.Services.AbuseProtection;

public sealed class AbuseProtectionRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AbuseProtectionOptions _options;
    private readonly AbuseProtectionLogger _logger;
    private readonly ConcurrentDictionary<string, FixedWindowRateLimiter> _limiters = new();

    public AbuseProtectionRateLimitMiddleware(
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
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var policyName = GetPolicyNameForPath(context.Request.Path);
        var policySettings = GetPolicySettings(policyName);
        var clientKey = AbuseProtectionClientKey.GetPartitionKey(context);
        var limiterKey = $"{policyName}:{clientKey}";
        var limiter = _limiters.GetOrAdd(
            limiterKey,
            _ => new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = policySettings.PermitLimit,
                QueueLimit = Math.Max(0, _options.QueueLimit),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                Window = policySettings.Window
            }));

        using var lease = limiter.AttemptAcquire(1);
        if (lease.IsAcquired)
        {
            await _next(context);
            return;
        }

        var retryAfterSeconds = GetRetryAfterSeconds(lease);
        _logger.LogRateLimitRejected(
            policyName,
            context.Request.Path.Value,
            context.Request.Method,
            clientKey,
            policySettings.PermitLimit,
            policySettings.Window.TotalSeconds,
            retryAfterSeconds);

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.Response.WriteAsJsonAsync(new { message = "Too many requests." });
    }

    private (int PermitLimit, TimeSpan Window) GetPolicySettings(string policyName)
    {
        return policyName switch
        {
            AbuseProtectionPolicyNames.ExpensiveAi => (
                Math.Max(1, _options.ExpensiveAiPermitLimitPerTenMinutes),
                TimeSpan.FromMinutes(10)),
            AbuseProtectionPolicyNames.LocalAts => (
                Math.Max(1, _options.LocalAtsPermitLimitPerMinute),
                TimeSpan.FromMinutes(1)),
            _ => (
                Math.Max(1, _options.GlobalPermitLimitPerMinute),
                TimeSpan.FromMinutes(1))
        };
    }

    private static string GetPolicyNameForPath(PathString path)
    {
        if (IsExpensiveAiPath(path))
        {
            return AbuseProtectionPolicyNames.ExpensiveAi;
        }

        if (IsLocalAtsPath(path))
        {
            return AbuseProtectionPolicyNames.LocalAts;
        }

        return AbuseProtectionPolicyNames.Global;
    }

    private static bool IsExpensiveAiPath(PathString path)
    {
        return path.StartsWithSegments("/api/resume-review") ||
            path.StartsWithSegments("/api/v1/resume-review") ||
            path.StartsWithSegments("/api/job-listings") ||
            path.StartsWithSegments("/api/v1/job-listings") ||
            path.StartsWithSegments("/api/ats-engine/keyword-extraction") ||
            path.StartsWithSegments("/api/v1/ats-engine/keyword-extraction") ||
            path.StartsWithSegments("/api/ats-engine/keyword-analysis") ||
            path.StartsWithSegments("/api/v1/ats-engine/keyword-analysis");
    }

    private static bool IsLocalAtsPath(PathString path)
    {
        return path.StartsWithSegments("/api/ats-engine/header-validation") ||
            path.StartsWithSegments("/api/v1/ats-engine/header-validation") ||
            path.StartsWithSegments("/api/ats-engine/keyword-scoring") ||
            path.StartsWithSegments("/api/v1/ats-engine/keyword-scoring") ||
            path.StartsWithSegments("/api/ats-engine/final-assessment") ||
            path.StartsWithSegments("/api/v1/ats-engine/final-assessment");
    }

    private static double? GetRetryAfterSeconds(RateLimitLease lease)
    {
        return lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
            ? Math.Ceiling(retryAfter.TotalSeconds)
            : null;
    }
}
