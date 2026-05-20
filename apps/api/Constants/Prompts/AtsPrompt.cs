
using ResumeReview.Api.Constants.Rubric;

namespace ResumeReview.Api.Constants.Prompts;


public class AtsPrompt
{

  public static string BuildAtsPrompt()
  {
    return $$"""
Analyse the attached resume PDF and return ONLY valid JSON.
Do not include markdown fences.
Do not include commentary outside the JSON object.

{{ScoringRubric.SharedScoringRubric}}

TASK:
Provide a section-by-section ATS and resume quality review.

INSTRUCTIONS:
- heading.score should represent the overall ATS/readability strength of the resume.
- resumeName should reflect the candidate name if visible in the resume; otherwise use "Candidate".
- Review these sections where present:


  - Professional summary
  - Work experience
  - Education
  - Projects
  - Skills
 
- If a section is missing but important, mention that as a weakness.
- summary should briefly explain the quality of the section.
- strengths should list what works well.
- weaknesses should list real issues only.
- suggestions should be practical improvements prioritised by severity.
- suggestedRewrites should only be included when genuinely useful.
- Do not invent missing resume content.
- Suggested rewrites must remain truthful.

Return ONLY this JSON shape:
{
  "heading": {
    "section": "string",
    "score": 0
  },
  "resumeName": "string",
  "content": [
    {
      "section": " Professional summary | Work experience | Education | Projects | Skills ",
      "summary": "string",
      "strengths": ["string"],
      "weaknesses": ["string"],
      "suggestions": [
        {
          "type": "Low | Medium | High",
          "content": "string"
        }
      ],
      "suggestedRewrites": [
        {
          "current": "string",
          "suggestion": "string",
          "expplanation": "string"
        }
      ],
      "score": 0
    }
  ]
}
""";
  }
}