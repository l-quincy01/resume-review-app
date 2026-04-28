using ResumeReview.Api.Constants.Prompts;
using ResumeReview.Api.Models;

using ResumeReview.Api.Services.Schemas;

namespace ResumeReview.Api.Services.Tasks;

public sealed class JobRecommendationTask : IAiAnalysisTask<JobRecommendation>
{
    public string SchemaName => "job_recommendation";

    public object Schema => JobRecommendationSchema.Schema;

    public string BuildPrompt(string? jobDescription = null)
    {
        return RecommendationPrompt.BuildJobRecommendationPrompt();
    }
}