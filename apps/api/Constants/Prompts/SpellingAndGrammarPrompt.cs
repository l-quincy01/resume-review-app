
using ResumeReview.Api.Constants.Rubric;

namespace ResumeReview.Api.Constants.Prompts;

public class SpellingAndGrammarPrompt
{


    public static string BuildSpellingAndGrammarPrompt()
    {
        return $$"""
Analyse the attached resume PDF and return ONLY valid JSON.
Do not include markdown fences.
Do not include commentary outside the JSON object.

{{ScoringRubric.SharedScoringRubric}}

TASK:
Review the resume only for writing quality and language issues.

INSTRUCTIONS:
- Focus only on grammar, spelling, personal pronouns, and passive voice.
- Do not perform ATS critique, career inference, or job-fit analysis.
- score should reflect overall writing quality.
- Only include genuine issues.
- Do not invent errors.
- Do not flag stylistic preference as an error unless it materially weakens the resume.
- Each suggestion must include:
  - type: Low, Medium, or High
  - current: exact problematic wording or short excerpt
  - suggestedCorrection: improved version
  - explanation: why it should change
- Return empty arrays when no valid issues exist.

Return ONLY this JSON shape:
{
  "score": 0,
  "grammarSuggestions": [
    {
      "type": "Low | Medium | High",
      "current": "string",
      "suggestedCorrection": "string",
      "explanation": "string"
    }
  ],
  "spellingSuggestions": [
    {
      "type": "Low | Medium | High",
      "current": "string",
      "suggestedCorrection": "string",
      "explanation": "string"
    }
  ],
  "personalPronounCheck": [
    {
      "type": "Low | Medium | High",
      "current": "string",
      "suggestedCorrection": "string",
      "explanation": "string"
    }
  ],
  "passiveVoiceCheck": [
    {
      "type": "Low | Medium | High",
      "current": "string",
      "suggestedCorrection": "string",
      "explanation": "string"
    }
  ]
}
""";
    }

}