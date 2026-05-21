namespace ResumeReview.Api.Services.Providers.OpenAI;

public sealed class OpenAiStructuredOutputException : InvalidOperationException
{
    public OpenAiStructuredOutputException(
        string schemaName,
        string model,
        int responseBodyLength,
        int outputLength,
        int? batchNumber,
        int keywordCount,
        string? jsonPath,
        long? lineNumber,
        long? bytePositionInLine,
        bool likelyTruncated,
        Exception innerException)
        : base(
            $"OpenAI structured output was not valid JSON for schema '{schemaName}'. OutputLength: {outputLength}.",
            innerException)
    {
        SchemaName = schemaName;
        Model = model;
        ResponseBodyLength = responseBodyLength;
        OutputLength = outputLength;
        BatchNumber = batchNumber;
        KeywordCount = keywordCount;
        JsonPath = jsonPath;
        LineNumber = lineNumber;
        BytePositionInLine = bytePositionInLine;
        LikelyTruncated = likelyTruncated;
    }

    public string SchemaName { get; }
    public string Model { get; }
    public int ResponseBodyLength { get; }
    public int OutputLength { get; }
    public int? BatchNumber { get; }
    public int KeywordCount { get; }
    public string? JsonPath { get; }
    public long? LineNumber { get; }
    public long? BytePositionInLine { get; }
    public bool LikelyTruncated { get; }
}
