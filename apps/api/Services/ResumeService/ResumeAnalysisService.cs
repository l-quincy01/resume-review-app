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
    private readonly AtsContentTask _atsContentTask;
    private readonly SpellingAndGrammarTask _spellingTask;
    private readonly JobSearchProfileTask _jobSearchProfileTask;

    public ResumeAnalysisService(
        IAiProviderClient aiProvider,
        ILogger<ResumeAnalysisService> logger,
        JobRecommendationTask jobRecommendationTask,
        AtsContentTask atsContentTask,
        SpellingAndGrammarTask spellingTask,
        JobSearchProfileTask jobSearchProfileTask)
    {
        _aiProvider = aiProvider;
        _logger = logger;
        _jobRecommendationTask = jobRecommendationTask;
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

        try
        {
            var jobRecommendationTask = RunTaskAsync(
                aiModel,
                fileId,
                _jobRecommendationTask,
                "job recommendations",
                null,
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
                atsContentTask,
                spellingTask,
                jobSearchProfileTask);

            return new ResumeReviewResponse
            {
                JobRecommendation = await jobRecommendationTask,
                AtsContent = await atsContentTask,
                SpellingAndGrammar = await spellingTask,
                JobSearchProfile = await jobSearchProfileTask,
                Warnings = warnings.ToList()
            };
        }
        finally
        {
            await CleanupUploadedFileAsync(fileId, cancellationToken);
        }
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

    private async Task CleanupUploadedFileAsync(
        string fileId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _aiProvider.DeleteFileAsync(fileId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "OpenAI file cleanup failed after resume analysis. FileId: {FileId}.",
                fileId);
        }
    }
}
