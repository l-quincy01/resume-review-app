using System.Net;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Options;

namespace ResumeReview.Api.Services.Providers.OpenAI;

public sealed class OpenAiRetryPolicy
{
    private static readonly HttpStatusCode[] TransientStatuses =
    [
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout
    ];

    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiRetryPolicy> _logger;
    private readonly object _circuitLock = new();
    private int _consecutiveFailures;
    private DateTimeOffset? _circuitOpenedUntil;

    public OpenAiRetryPolicy(
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiRetryPolicy> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> SendAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> sendAsync,
        string operationName,
        CancellationToken cancellationToken)
    {
        ThrowIfCircuitOpen(operationName);
        var maxAttempts = Math.Max(1, _options.MaxRetries + 1);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            HttpResponseMessage? response = null;

            try
            {
                response = await sendAsync(cancellationToken);

                if (!IsTransient(response.StatusCode))
                {
                    RecordSuccess();
                    return response;
                }

                if (attempt == maxAttempts)
                {
                    RecordFailure(operationName);
                    return response;
                }

                _logger.LogWarning(
                    "Transient OpenAI failure for {Operation}. Status: {Status}. Attempt {Attempt}/{MaxAttempts}.",
                    operationName,
                    response.StatusCode,
                    attempt,
                    maxAttempts);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (IsRetriableException(ex) && attempt < maxAttempts)
            {
                RecordFailure(operationName);
                _logger.LogWarning(
                    ex,
                    "Transient OpenAI exception for {Operation}. Attempt {Attempt}/{MaxAttempts}.",
                    operationName,
                    attempt,
                    maxAttempts);
            }
            catch (Exception ex) when (IsRetriableException(ex))
            {
                RecordFailure(operationName);
                throw;
            }
            finally
            {
                if (response is not null && IsTransient(response.StatusCode) && attempt < maxAttempts)
                {
                    response.Dispose();
                }
            }

            await Task.Delay(GetDelay(attempt), cancellationToken);
        }

        throw new InvalidOperationException($"OpenAI retry policy exhausted for {operationName}.");
    }

    private static bool IsTransient(HttpStatusCode statusCode)
    {
        return TransientStatuses.Contains(statusCode);
    }

    private static bool IsRetriableException(Exception exception)
    {
        return exception is HttpRequestException or TaskCanceledException or TimeoutException;
    }

    private TimeSpan GetDelay(int attempt)
    {
        var baseDelay = Math.Max(100, _options.RetryBaseDelayMilliseconds);
        var jitter = Random.Shared.Next(0, baseDelay);
        var exponentialDelay = baseDelay * Math.Pow(2, attempt - 1);

        return TimeSpan.FromMilliseconds(exponentialDelay + jitter);
    }

    private void ThrowIfCircuitOpen(string operationName)
    {
        lock (_circuitLock)
        {
            if (_circuitOpenedUntil is null || _circuitOpenedUntil <= DateTimeOffset.UtcNow)
            {
                return;
            }

            throw new InvalidOperationException(
                $"OpenAI circuit is open for {operationName} until {_circuitOpenedUntil:O}.");
        }
    }

    private void RecordSuccess()
    {
        lock (_circuitLock)
        {
            _consecutiveFailures = 0;
            _circuitOpenedUntil = null;
        }
    }

    private void RecordFailure(string operationName)
    {
        lock (_circuitLock)
        {
            _consecutiveFailures++;

            if (_consecutiveFailures < Math.Max(1, _options.CircuitBreakerFailureThreshold))
            {
                return;
            }

            _circuitOpenedUntil = DateTimeOffset.UtcNow.AddSeconds(
                Math.Max(1, _options.CircuitBreakerBreakSeconds));

            _logger.LogError(
                "OpenAI circuit opened for {Operation} after {FailureCount} consecutive failures until {OpenedUntil}.",
                operationName,
                _consecutiveFailures,
                _circuitOpenedUntil);
        }
    }
}
