using ResumeReview.Api.Services.Providers.OpenAI;

namespace ResumeReview.Api.Tests;

public class OpenAiErrorInfoTests
{
    [Fact]
    public void Parse_ReturnsSanitizedProviderErrorMetadata()
    {
        const string responseJson = """
        {
          "error": {
            "message": "Unsupported parameter: temperature.",
            "type": "invalid_request_error",
            "param": "temperature",
            "code": "unsupported_parameter"
          }
        }
        """;

        var error = OpenAiErrorInfo.Parse(responseJson);

        Assert.Equal("invalid_request_error", error.Type);
        Assert.Equal("unsupported_parameter", error.Code);
        Assert.Equal("temperature", error.Param);
        Assert.Equal("Unsupported parameter: temperature.", error.Message);
    }

    [Fact]
    public void Parse_ReturnsEmptyMetadataForNonJsonBody()
    {
        var error = OpenAiErrorInfo.Parse("provider failure");

        Assert.Null(error.Type);
        Assert.Null(error.Code);
        Assert.Null(error.Param);
        Assert.Null(error.Message);
    }
}
