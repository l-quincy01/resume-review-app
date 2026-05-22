using ResumeReview.Api.Constants.Prompts;
using ResumeReview.Api.Models;

using ResumeReview.Api.Services.Schemas;

namespace ResumeReview.Api.Services.Tasks;

public sealed class AtsContentTask : IAiAnalysisTask<AtsContent>
{
    public string SchemaName => "ats_content";

    public object Schema => AtsContentSchema.Schema;

    public string BuildPrompt(string? jobDescription = null)
    {
        return AtsPrompt.BuildAtsPrompt();
    }
}
