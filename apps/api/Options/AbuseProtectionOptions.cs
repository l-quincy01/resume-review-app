namespace ResumeReview.Api.Options;

public sealed class AbuseProtectionOptions
{
    public const string SectionName = "AbuseProtection";

    public int GlobalPermitLimitPerMinute { get; set; } = 120;
    public int ExpensiveAiPermitLimitPerTenMinutes { get; set; } = 10;
    public int LocalAtsPermitLimitPerMinute { get; set; } = 30;
    public long MaxMultipartBodyBytes { get; set; } = 6 * 1024 * 1024;
    public long MaxJsonBodyBytes { get; set; } = 128 * 1024;
    public int QueueLimit { get; set; } = 0;
}
