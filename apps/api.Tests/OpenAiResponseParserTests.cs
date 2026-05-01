using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Tests;

public class OpenAiResponseParserTests
{
    [Fact]
    public void ExtractTextOutput_ConcatenatesOutputText()
    {
        const string responseJson = """
        {
          "output": [
            {
              "type": "message",
              "content": [
                { "type": "output_text", "text": "{\"score\":90}" },
                { "type": "output_text", "text": "{\"next\":true}" }
              ]
            }
          ]
        }
        """;

        var result = OpenAiResponseParser.ExtractTextOutput(responseJson);

        Assert.Equal("{\"score\":90}{\"next\":true}", result);
    }

    [Fact]
    public void ExtractTextOutput_ThrowsWhenOutputArrayMissing()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => OpenAiResponseParser.ExtractTextOutput("""{ "id": "resp_123" }"""));

        Assert.Contains("output array", exception.Message);
    }

    [Fact]
    public void ExtractTextOutput_ThrowsWhenNoOutputTextExists()
    {
        const string responseJson = """
        {
          "output": [
            {
              "type": "message",
              "content": [
                { "type": "refusal", "text": "no" }
              ]
            }
          ]
        }
        """;

        var exception = Assert.Throws<InvalidOperationException>(
            () => OpenAiResponseParser.ExtractTextOutput(responseJson));

        Assert.Contains("No output_text", exception.Message);
    }
}
