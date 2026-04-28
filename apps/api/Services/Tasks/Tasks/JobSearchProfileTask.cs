using ResumeReview.Api.Constants.Prompts;
using ResumeReview.Api.Models;

using ResumeReview.Api.Services.Schemas;

namespace ResumeReview.Api.Services.Tasks;

public sealed class JobSearchProfileTask : IAiAnalysisTask<JobSearchProfile>
{
    public string SchemaName => "job_search_profile";

    public object Schema => JobSearchProfileSchema.Schema;

    public string BuildPrompt(string? jobDescription = null)
    {
        return JobSearchProfilePrompt.BuildJobSearchProfilePrompt();
    }
}