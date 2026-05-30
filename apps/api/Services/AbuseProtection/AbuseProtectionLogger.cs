namespace ResumeReview.Api.Services.AbuseProtection;

public sealed class AbuseProtectionLogger
{
    private readonly ILogger<AbuseProtectionLogger> _logger;

    public AbuseProtectionLogger(ILogger<AbuseProtectionLogger> logger)
    {
        _logger = logger;
    }

    public void LogRateLimitRejected(
        string policyName,
        string? requestPath,
        string requestMethod,
        string clientKey,
        int permitLimit,
        double windowSeconds,
        double? retryAfterSeconds)
    {
        _logger.LogWarning(
            "Request rejected by rate limit. Policy: {PolicyName}. Path: {RequestPath}. Method: {RequestMethod}. ClientKey: {ClientKey}. PermitLimit: {PermitLimit}. WindowSeconds: {WindowSeconds}. RetryAfterSeconds: {RetryAfterSeconds}.",
            policyName,
            requestPath,
            requestMethod,
            clientKey,
            permitLimit,
            windowSeconds,
            retryAfterSeconds);
    }

    public void LogRequestBodyRejected(
        string? requestPath,
        string requestMethod,
        string? contentType,
        long maxBytes,
        long? contentLength)
    {
        _logger.LogWarning(
            "Request body rejected by abuse protection. Path: {RequestPath}. Method: {RequestMethod}. ContentType: {ContentType}. MaxBytes: {MaxBytes}. ContentLength: {ContentLength}.",
            requestPath,
            requestMethod,
            contentType,
            maxBytes,
            contentLength);
    }
}
