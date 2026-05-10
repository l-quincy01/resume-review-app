namespace ResumeReview.Api.Services.Schemas;

public static class AtsKeywordExtractionSchema
{
    public static object Schema => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            job_title = new { type = "string" },
            keywords = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        keyword = new { type = "string" },
                        tier = new
                        {
                            type = "integer",
                            @enum = new[] { 1, 2, 3 }
                        },
                        requirement = new
                        {
                            type = "string",
                            @enum = new[] { "must_have", "nice_to_have" }
                        },
                        category = new
                        {
                            type = "string",
                            @enum = new[]
                            {
                                "technical_skill",
                                "tool",
                                "framework",
                                "language",
                                "methodology",
                                "domain_keyword",
                                "role_specific_requirement",
                                "soft_skill",
                                "generic_business_term",
                                "other"
                            }
                        },
                        context = new { type = "string" },
                        variations = new
                        {
                            type = "array",
                            items = new { type = "string" }
                        }
                    },
                    required = new[]
                    {
                        "keyword",
                        "tier",
                        "requirement",
                        "category",
                        "context",
                        "variations"
                    }
                }
            }
        },
        required = new[] { "job_title", "keywords" }
    };
}
