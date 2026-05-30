using System.Security.Cryptography;
using System.Text;

namespace ResumeReview.Api.Services.AbuseProtection;

public static class AbuseProtectionClientKey
{
    public static string GetPartitionKey(HttpContext context)
    {
        var identifier =
            GetFirstForwardedFor(context) ??
            GetHeaderValue(context, "X-Real-IP") ??
            context.Connection.RemoteIpAddress?.ToString() ??
            "unknown";

        return Hash(identifier);
    }

    private static string? GetFirstForwardedFor(HttpContext context)
    {
        var forwardedFor = GetHeaderValue(context, "X-Forwarded-For");
        return forwardedFor?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
    }

    private static string? GetHeaderValue(HttpContext context, string headerName)
    {
        var value = context.Request.Headers[headerName].ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }
}
