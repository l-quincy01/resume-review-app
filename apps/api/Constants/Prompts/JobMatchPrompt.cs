
using ResumeReview.Api.Constants.Rubric;

namespace ResumeReview.Api.Constants.Prompts;


public class JobMatchPrompt
{

  public static string BuildJobMatchPrompt(string? jobDescription)
  {
    var jdBlock = string.IsNullOrWhiteSpace(jobDescription)
        ? """
No job description was provided.

Set:
- "name" to null
- "targetJob" to []
- "overallScore" to 0
"""
        : $"""
Use this target job description as context:

{jobDescription}
""";

    return $$"""
Analyse the attached resume PDF and return ONLY valid JSON.
Do not include markdown fences.
Do not include commentary outside the JSON object.

{{ScoringRubric.SharedScoringRubric}}

TASK:
Evaluate how well the candidate matches the provided target job description.

{{jdBlock}}

INSTRUCTIONS:
- Use only these allowed type values:
  - Skills Match
  - Keywords & ATS Optimization
  - Education & Qualifications
  - Industry/Domain Relevance
  - Job Title Alignment
  - Seniority/Experience Level
  - Accomplishments & Metrics
  - Cultural & Values Fit Signals
- Each targetJob item must explain the match or mismatch clearly.
- overallScore must reflect the overall fit honestly, not optimistically.
- Penalise missing core requirements, weak keyword alignment, lack of metrics, weak title alignment, and insufficient experience where relevant.

Return ONLY this JSON shape:
{
  "name": "string or null",
  "targetJob": [
    {
      "score": 0,
      "type":  "Skills Match"| "Keywords & ATS Optimization"| "Education & Qualifications"| "Industry/Domain Relevance"| "Job Title Alignment"| "Seniority/Experience Level"| "Accomplishments & Metrics"| "Cultural & Values Fit ",
      "content": "string"
    }
  ],
  "overallScore": 0
}
""";
  }
}