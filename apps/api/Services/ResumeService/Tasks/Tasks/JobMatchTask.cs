using ResumeReview.Api.Constants.Prompts;
using ResumeReview.Api.Models;

using ResumeReview.Api.Services.Schemas;

namespace ResumeReview.Api.Services.Tasks;

public sealed class JobMatchTask : IAiAnalysisTask<JobMatch>
{
    public string SchemaName => "job_match";

    public object Schema => JobMatchSchema.Schema;

    public string BuildPrompt(string? jobDescription = null)
    {
        return JobMatchPrompt.BuildJobMatchPrompt(jobDescription);
    }
}