
namespace ResumeReview.Api.Services.Schemas;

public class AtsContentSchema
{

    public static object Schema => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            heading = new
            {
                type = "object",
                additionalProperties = false,
                properties = new
                {
                    section = new { type = "string" },
                    score = new { type = "integer" }
                },
                required = new[] { "section", "score" }
            },
            resumeName = new { type = "string" },
            content = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        section = new { type = "string" },
                        summary = new { type = "string" },
                        strengths = new { type = "array", items = new { type = "string" } },
                        weaknesses = new { type = "array", items = new { type = "string" } },
                        suggestions = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                additionalProperties = false,
                                properties = new
                                {
                                    type = new { type = "string" },
                                    content = new { type = "string" }
                                },
                                required = new[] { "type", "content" }
                            }
                        },
                        suggestedRewrites = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                additionalProperties = false,
                                properties = new
                                {
                                    current = new { type = "string" },
                                    suggestion = new { type = "string" },
                                    expplanation = new { type = "string" }
                                },
                                required = new[] { "current", "suggestion", "expplanation" }
                            }
                        },
                        score = new { type = "integer" }
                    },
                    required = new[]
                 {
                            "section",
                            "summary",
                            "strengths",
                            "weaknesses",
                            "suggestions",
                            "suggestedRewrites",
                            "score"
                        }
                }
            }
        },
        required = new[] { "heading", "resumeName", "content" }
    };

}