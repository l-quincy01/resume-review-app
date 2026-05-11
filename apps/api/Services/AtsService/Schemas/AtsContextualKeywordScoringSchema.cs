namespace ResumeReview.Api.Services.AtsService.Schemas;

public static class AtsContextualKeywordScoringSchema
{
    public static object Schema => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            keyword_scores = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        keyword = new { type = "string" },
                        present = new { type = "boolean" },
                        matched_terms = new
                        {
                            type = "array",
                            items = new { type = "string" }
                        },
                        context_type = new
                        {
                            type = "object",
                            additionalProperties = false,
                            properties = new
                            {
                                has_achievement = new { type = "boolean" },
                                has_metric = new { type = "boolean" },
                                has_action_verb = new { type = "boolean" },
                                in_experience_section = new { type = "boolean" },
                                in_project_section = new { type = "boolean" },
                                in_summary_section = new { type = "boolean" }
                            },
                            required = new[]
                            {
                                "has_achievement",
                                "has_metric",
                                "has_action_verb",
                                "in_experience_section",
                                "in_project_section",
                                "in_summary_section"
                            }
                        },
                        evidence = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                additionalProperties = false,
                                properties = new
                                {
                                    section = new { type = "string" },
                                    text = new { type = "string" },
                                    matched_term = new { type = "string" }
                                },
                                required = new[] { "section", "text", "matched_term" }
                            }
                        }
                    },
                    required = new[]
                    {
                        "keyword",
                        "present",
                        "matched_terms",
                        "context_type",
                        "evidence"
                    }
                }
            }
        },
        required = new[] { "keyword_scores" }
    };
}
