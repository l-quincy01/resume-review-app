
namespace ResumeReview.Api.Services.Schemas;

public class JobMatchSchema
{
    public static object Schema => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            name = new { type = new[] { "string", "null" } },
            targetJob = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        score = new { type = "integer" },
                        type = new { type = "string" },
                        content = new { type = "string" }
                    },
                    required = new[] { "score", "type", "content" }
                }
            },
            overallScore = new { type = "integer" }
        },
        required = new[] { "name", "targetJob", "overallScore" }
    };
}