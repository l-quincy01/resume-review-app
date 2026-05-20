namespace ResumeReview.Api.Services.Providers.OpenAI;

public static class OpenAiResponsesRequestFactory
{
    public static Dictionary<string, object?> CreateStructuredRequest(
        string model,
        object[] input,
        string schemaName,
        object schema,
        int? maxOutputTokens = null,
        string? promptCacheKey = null)
    {
        var request = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["input"] = input,
            ["text"] = new
            {
                verbosity = "low",
                format = new
                {
                    type = "json_schema",
                    name = schemaName,
                    strict = true,
                    schema
                }
            }
        };

        if (SupportsTemperature(model))
        {
            request["temperature"] = 0;
        }

        if (maxOutputTokens is > 0)
        {
            request["max_output_tokens"] = maxOutputTokens.Value;
        }

        if (IsGpt5Model(model))
        {
            request["reasoning"] = new { effort = "minimal" };
        }

        if (!string.IsNullOrWhiteSpace(promptCacheKey))
        {
            request["prompt_cache_key"] = promptCacheKey;
        }

        return request;
    }

    private static bool IsGpt5Model(string model)
    {
        return model.StartsWith("gpt-5", StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsTemperature(string model)
    {
        return !IsGpt5Model(model);
    }
}
