
using ResumeReview.Api.Constants.Rubric;

namespace ResumeReview.Api.Constants.Prompts;

public class RecommendationPrompt
{



    public static string BuildJobRecommendationPrompt()
    {
        return $$"""
Analyse the attached resume PDF and return ONLY valid JSON.
Do not include markdown fences.
Do not include commentary outside the JSON object.

{{ScoringRubric.SharedScoringRubric}}

TASK:
Infer the most realistic job directions for this candidate based only on the resume.

INSTRUCTIONS:
- Estimate yearsExperience realistically from the evidence in the resume.
- jobTitles should contain realistic target job titles the candidate could reasonably apply for now.
- responsibilities should list likely responsibilities the candidate is prepared to perform based on demonstrated experience.
- seniority should reflect realistic level only, not aspirational level.
- industry should include sectors the candidate is best aligned to.
- hardSkills should include concrete technical or domain skills directly supported by the resume.
- softSkills should include only soft skills reasonably evidenced by the resume.
- companySizeFit should suggest the types of organisations the candidate may fit well into.
- careerTrack should suggest realistic next-step career directions.
- Do not include duplicates.
- Do not include exaggerated or unsupported roles.

Return ONLY this JSON shape:
{
  "yearsExperience": "string",
  "jobTitles": ["string"],
  "responsibilities": ["string"],
  "seniority": ["string"],
  "industry": ["string"],
  "hardSkills": ["string"],
  "softSkills": ["string"],
  "companySizeFit": ["string"],
  "careerTrack": ["string"]
}
""";
    }

}