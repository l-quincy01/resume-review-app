
namespace ResumeReview.Api.Services.Schemas;

public static class SpellingAndGrammarSchema
{

    public static object Schema => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            score = new { type = "integer" },
            grammarSuggestions = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        type = new { type = "string" },
                        current = new { type = "string" },
                        suggestedCorrection = new { type = "string" },
                        explanation = new { type = "string" }
                    },
                    required = new[] { "type", "current", "suggestedCorrection", "explanation" }
                }
            },
            spellingSuggestions = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        type = new { type = "string" },
                        current = new { type = "string" },
                        suggestedCorrection = new { type = "string" },
                        explanation = new { type = "string" }
                    },
                    required = new[] { "type", "current", "suggestedCorrection", "explanation" }
                }
            },
            personalPronounCheck = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        type = new { type = "string" },
                        current = new { type = "string" },
                        suggestedCorrection = new { type = "string" },
                        explanation = new { type = "string" }
                    },
                    required = new[] { "type", "current", "suggestedCorrection", "explanation" }
                }
            },
            passiveVoiceCheck = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        type = new { type = "string" },
                        current = new { type = "string" },
                        suggestedCorrection = new { type = "string" },
                        explanation = new { type = "string" }
                    },
                    required = new[] { "type", "current", "suggestedCorrection", "explanation" }
                }
            }
        },
        required = new[]
            {
                "score",
                "grammarSuggestions",
                "spellingSuggestions",
                "personalPronounCheck",
                "passiveVoiceCheck"
            }
    };



}