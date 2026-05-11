using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;



namespace ResumeReview.Api.Services.Providers.OpenAI;


public class OpenAiResponseParser
{

    public static string ExtractTextOutput(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);

        if (!doc.RootElement.TryGetProperty("output", out var outputElement) ||
            outputElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("OpenAI response did not contain an output array.");
        }

        var sb = new StringBuilder();

        foreach (var outputItem in outputElement.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("type", out var itemType) ||
                itemType.GetString() != "message")
            {
                continue;
            }

            if (!outputItem.TryGetProperty("content", out var contentElement) ||
                contentElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in contentElement.EnumerateArray())
            {
                if (!contentItem.TryGetProperty("type", out var contentType) ||
                    contentType.GetString() != "output_text")
                {
                    continue;
                }

                if (contentItem.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.Append(text);
                    }
                }
            }
        }

        var result = sb.ToString().Trim();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException(
                $"No output_text content was found in the OpenAI response. Raw response: {responseJson}");
        }

        return result;
    }
}