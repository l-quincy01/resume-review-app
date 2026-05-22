namespace ResumeReview.Api.Options;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "gpt-4.1-mini";
    public string JobListingsModel { get; set; } = "gpt-5-nano";
    public string[] AllowedModels { get; set; } = ["gpt-4.1-mini", "gpt-5-mini", "gpt-5.4", "chat-latest", "gpt-5-nano"];
    public int RequestTimeoutSeconds { get; set; } = 180;
    public int JobListingsTimeoutSeconds { get; set; } = 180;
    public int ResumeReviewMaxOutputTokens { get; set; } = 3000;
    public int KeywordExtractionMaxOutputTokens { get; set; } = 2500;
    public int KeywordAnalysisMaxOutputTokens { get; set; } = 8000;
    public int KeywordAnalysisBatchSize { get; set; } = 10;
    public int MaxRetries { get; set; } = 2;
    public int RetryBaseDelayMilliseconds { get; set; } = 500;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public int CircuitBreakerBreakSeconds { get; set; } = 30;
    public long MaxResumeBytes { get; set; } = 5 * 1024 * 1024;
    public int MaxJobDescriptionCharacters { get; set; } = 12000;
    public int MaxJobListings { get; set; } = 5;
}
