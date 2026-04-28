
using ResumeReview.Api.Dtos.Responses;

using ResumeReview.Api.Models;

using ResumeReview.Api.Services.Tasks;
using ResumeReview.Api.Services.Providers.OpenAI;
using ResumeReview.Api.Services.Providers;

namespace ResumeReview.Api.Services;

public sealed class ResumeAnalysisService : IAiResumeAnalysisService
{
    private readonly IAiProviderClient _aiProvider;
    private readonly ILogger<ResumeAnalysisService> _logger;

    private readonly JobRecommendationTask _jobRecommendationTask;
    private readonly JobMatchTask _jobMatchTask;
    private readonly AtsContentTask _atsContentTask;
    private readonly SpellingAndGrammarTask _spellingTask;
    private readonly JobSearchProfileTask _jobSearchProfileTask;

    public ResumeAnalysisService(
        IAiProviderClient aiProvider,
        ILogger<ResumeAnalysisService> logger,
        JobRecommendationTask jobRecommendationTask,
        JobMatchTask jobMatchTask,
        AtsContentTask atsContentTask,
        SpellingAndGrammarTask spellingTask,
        JobSearchProfileTask jobSearchProfileTask)
    {
        _aiProvider = aiProvider;
        _logger = logger;
        _jobRecommendationTask = jobRecommendationTask;
        _jobMatchTask = jobMatchTask;
        _atsContentTask = atsContentTask;
        _spellingTask = spellingTask;
        _jobSearchProfileTask = jobSearchProfileTask;
    }

    public async Task<ResumeReviewResponse> AnalyzeResumeAsync(
        string aiModel,
        Stream pdfStream,
        string fileName,
        string? contentType,
        string? jobDescription,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting resume analysis for file {FileName}", fileName);

        var fileId = await _aiProvider.UploadFileAsync(
            pdfStream,
            fileName,
            contentType ?? "application/pdf",
            cancellationToken);

        var jobRecommendationTask = RunTaskAsync<JobRecommendation>(
            aiModel,
            fileId,
            _jobRecommendationTask,
            null,
            cancellationToken);

        var jobMatchTask = RunTaskAsync<JobMatch>(
            aiModel,
            fileId,
            _jobMatchTask,
            jobDescription,
            cancellationToken);

        var atsContentTask = RunTaskAsync<AtsContent>(
            aiModel,
            fileId,
            _atsContentTask,
            null,
            cancellationToken);

        var spellingTask = RunTaskAsync<SpellingAndGrammar>(
            aiModel,
            fileId,
            _spellingTask,
            null,
            cancellationToken);

        var jobSearchProfileTask = RunTaskAsync<JobSearchProfile>(
            aiModel,
            fileId,
            _jobSearchProfileTask,
            null,
            cancellationToken);

        await Task.WhenAll(
            jobRecommendationTask,
            jobMatchTask,
            atsContentTask,
            spellingTask,
            jobSearchProfileTask);

        return new ResumeReviewResponse
        {
            JobRecommendation = await jobRecommendationTask,
            JobMatch = await jobMatchTask,
            AtsContent = await atsContentTask,
            SpellingAndGrammar = await spellingTask,
            JobSearchProfile = await jobSearchProfileTask
        };
    }

    private Task<T> RunTaskAsync<T>(
        string aiModel,
        string fileId,
        IAiAnalysisTask<T> task,
        string? jobDescription,
        CancellationToken cancellationToken)
    {
        return _aiProvider.SendStructuredRequestAsync<T>(
            aiModel,
            fileId,
            task.BuildPrompt(jobDescription),
            task.SchemaName,
            task.Schema,
            cancellationToken);
    }
}