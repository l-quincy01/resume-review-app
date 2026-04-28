
namespace ResumeReview.Api.Services.Schemas;

public class JobSearchProfileSchema
{

    public static object Schema => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            titles = new { type = "array", items = new { type = "string" } },
            keywords = new { type = "array", items = new { type = "string" } },
            seniority = new { type = "string" },
            locations = new { type = "array", items = new { type = "string" } },
            exclude = new { type = "array", items = new { type = "string" } }
        },
        required = new[] { "titles", "keywords", "seniority", "locations", "exclude" }
    };
}