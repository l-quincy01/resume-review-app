using System.Collections.Concurrent;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Models;
using ResumeReview.Api.Services.Tasks;
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
        var warnings = new ConcurrentBag<string>();

        var fileId = await _aiProvider.UploadFileAsync(
            pdfStream,
            fileName,
            contentType ?? "application/pdf",
            cancellationToken);

        var jobRecommendationTask = RunTaskAsync(
            aiModel,
            fileId,
            _jobRecommendationTask,
            "job recommendations",
            null,
            warnings,
            cancellationToken);

        var jobMatchTask = RunTaskAsync(
            aiModel,
            fileId,
            _jobMatchTask,
            "job match",
            jobDescription,
            warnings,
            cancellationToken);

        var atsContentTask = RunTaskAsync(
            aiModel,
            fileId,
            _atsContentTask,
            "ATS content",
            null,
            warnings,
            cancellationToken);

        var spellingTask = RunTaskAsync(
            aiModel,
            fileId,
            _spellingTask,
            "spelling and grammar",
            null,
            warnings,
            cancellationToken);

        var jobSearchProfileTask = RunTaskAsync(
            aiModel,
            fileId,
            _jobSearchProfileTask,
            "job search profile",
            null,
            warnings,
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
            JobSearchProfile = await jobSearchProfileTask,
            Warnings = warnings.ToList()
        };
    }

    private async Task<T> RunTaskAsync<T>(
        string aiModel,
        string fileId,
        IAiAnalysisTask<T> task,
        string sectionName,
        string? jobDescription,
        ConcurrentBag<string> warnings,
        CancellationToken cancellationToken)
        where T : new()
    {
        try
        {
            return await _aiProvider.SendStructuredRequestAsync<T>(
                aiModel,
                fileId,
                task.BuildPrompt(jobDescription),
                task.SchemaName,
                task.Schema,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resume analysis section {SectionName} failed.", sectionName);
            warnings.Add($"{sectionName} could not be generated. Other report sections may still be usable.");

            return new T();
        }
    }
}
