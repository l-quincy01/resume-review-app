

namespace ResumeReview.Api.Constants.Rubric;

public class ScoringRubric
{
    public const string SharedScoringRubric = """
SCORING RUBRIC (apply consistently across all calls):
- All scores must be integers from 0 to 100.
- 90-100: exceptional, clear evidence, strong relevance, minimal weaknesses.
- 75-89: strong, credible, but with a few meaningful gaps or missed optimisation opportunities.
- 60-74: decent but clearly limited by missing detail, weak phrasing, low specificity, or incomplete evidence.
- 40-59: below competitive standard; notable weaknesses reduce effectiveness.
- 20-39: poor quality, weak alignment, major omissions, or serious writing/structure issues.
- 0-19: unusable, absent, or completely unsupported by the resume/job description.

GROUNDING RULES:
- Use only evidence supported by the attached resume PDF and the provided job description when one exists.
- Do not invent achievements, metrics, technologies, certifications, industries, or responsibilities.
- If something is missing, score accordingly instead of guessing.
- Be realistic, not aspirational.
- Penalise vague phrasing, lack of metrics, weak title alignment, missing keywords, poor structure, and unsupported claims.
- Prefer concise, direct, professional wording.
- Return empty arrays when no valid items exist.
""";




}