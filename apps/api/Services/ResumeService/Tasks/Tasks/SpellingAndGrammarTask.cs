using ResumeReview.Api.Constants.Prompts;
using ResumeReview.Api.Models;

using ResumeReview.Api.Services.Schemas;

namespace ResumeReview.Api.Services.Tasks;

public sealed class SpellingAndGrammarTask : IAiAnalysisTask<SpellingAndGrammar>
{
    public string SchemaName => "spelling_and_grammar";

    public object Schema => SpellingAndGrammarSchema.Schema;

    public string BuildPrompt(string? jobDescription = null)
    {
        return SpellingAndGrammarPrompt.BuildSpellingAndGrammarPrompt();
    }
}