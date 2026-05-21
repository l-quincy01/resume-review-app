using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Tests;

public class OpenAiJsonResponseDeserializerTests
{
    [Fact]
    public void DeserializeStructuredOutput_DeserializesValidOutputText()
    {
        var responseJson = CreateResponse("""{"keyword_scores":[]}""", outputTokens: 10);

        var result = OpenAiJsonResponseDeserializer.DeserializeStructuredOutput<KeywordAnalysisResponse>(
            responseJson,
            "ats_contextual_keyword_scoring",
            "gpt-5-mini",
            OpenAiResponseUsageParser.Parse(responseJson),
            maxOutputTokens: 100,
            batchNumber: 1,
            keywordCount: 2);

        Assert.Empty(result.KeywordScores);
    }

    [Fact]
    public void DeserializeStructuredOutput_ThrowsMetadataExceptionForMalformedJsonWithoutRawOutput()
    {
        var responseJson = CreateResponse("""{"keyword_scores":[""", outputTokens: 96);

        var exception = Assert.Throws<OpenAiStructuredOutputException>(() =>
            OpenAiJsonResponseDeserializer.DeserializeStructuredOutput<KeywordAnalysisResponse>(
                responseJson,
                "ats_contextual_keyword_scoring",
                "gpt-5-mini",
                OpenAiResponseUsageParser.Parse(responseJson),
                maxOutputTokens: 100,
                batchNumber: 3,
                keywordCount: 10));

        Assert.Equal("ats_contextual_keyword_scoring", exception.SchemaName);
        Assert.Equal("gpt-5-mini", exception.Model);
        Assert.Equal(3, exception.BatchNumber);
        Assert.Equal(10, exception.KeywordCount);
        Assert.True(exception.LikelyTruncated);
        Assert.DoesNotContain("keyword_scores", exception.Message);
    }

    private static string CreateResponse(string outputText, int outputTokens)
    {
        return $$"""
        {
          "output": [
            {
              "type": "message",
              "content": [
                {
                  "type": "output_text",
                  "text": {{System.Text.Json.JsonSerializer.Serialize(outputText)}}
                }
              ]
            }
          ],
          "usage": {
            "input_tokens": 10,
            "output_tokens": {{outputTokens}},
            "total_tokens": 20,
            "input_tokens_details": {
              "cached_tokens": 0
            }
          }
        }
        """;
    }
}
