
using ResumeReview.Api.Constants.Rubric;

namespace ResumeReview.Api.Constants.Prompts;


public class JobSearchProfilePrompt
{

    public static string BuildJobSearchProfilePrompt()
    {
        return $$"""
Analyse the attached resume PDF and return ONLY valid JSON.
Do not include markdown fences.
Do not include commentary outside the JSON object.

{{ScoringRubric.SharedScoringRubric}}

TASK:
Derive a focused job-search profile from the resume so that a later web-search step can find better job listings.

INSTRUCTIONS:
- Infer the candidate's most realistic current target job titles.
- Infer the most relevant hard-skill search keywords directly supported by the resume.
- Infer realistic seniority only.
- Infer the best-fit search locations.
- Infer which titles or levels should be excluded from search.
- Keep everything realistic and grounded only in the resume.
- Do not include aspirational roles.
- Do not include duplicates.
- Prefer concise, search-friendly terms.

RETURN RULES:
- titles: 3 to 5 realistic target titles
- keywords: 5 to 10 strong search keywords
- seniority: short phrase only
- locations: 3 to 6 relevant locations
- exclude: 4 to 8 roles/levels/terms to avoid

Return ONLY this JSON shape:
{
  "titles": ["string"],
  "keywords": ["string"],
  "seniority": "string",
  "locations": ["string"],
  "exclude": ["string"]
}
""";
    }
}