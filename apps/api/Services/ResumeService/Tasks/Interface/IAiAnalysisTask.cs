namespace ResumeReview.Api.Services.Tasks;

public interface IAiAnalysisTask<T>
{
    string SchemaName { get; }

    object Schema { get; }

    string BuildPrompt(string? jobDescription = null);
}