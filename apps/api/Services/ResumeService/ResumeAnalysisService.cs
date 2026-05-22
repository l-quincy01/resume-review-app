using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Models;
using ResumeReview.Api.Services.Tasks;
using ResumeReview.Api.Services.Providers;

namespace ResumeReview.Api.Services;

public sealed class ResumeAnalysisService : IAiResumeAnalysisService
{
    private const string AtsContentSection = "ats_content";
    private const string SpellingAndGrammarSection = "spelling_and_grammar";
    private const string JobRecommendationSection = "job_recommendation";
    private const string JobSearchProfileSection = "job_search_profile";

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

    public async IAsyncEnumerable<ResumeReviewStreamEnvelope> AnalyzeResumeStreamAsync(
        string aiModel,
        Stream pdfStream,
        string fileName,
        string? contentType,
        string? jobDescription,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting streamed resume analysis for file {FileName}", fileName);
        var warnings = new ConcurrentBag<string>();
        var completedSections = new List<string>();
        var response = new ResumeReviewResponse();

        yield return CreateEvent("review_started", null, null, null, completedSections);

        var fileId = await _aiProvider.UploadFileAsync(
            pdfStream,
            fileName,
            contentType ?? "application/pdf",
            cancellationToken);

        try
        {
            var sectionTasks = new List<SectionWork>
            {
                CreateSectionWork(
                    JobRecommendationSection,
                    "job recommendations",
                    RunStreamTaskAsync(
                        aiModel,
                        fileId,
                        _jobRecommendationTask,
                        "job recommendations",
                        null,
                        warnings,
                        cancellationToken)),
                CreateSectionWork(
                    AtsContentSection,
                    "ATS content",
                    RunStreamTaskAsync(
                        aiModel,
                        fileId,
                        _atsContentTask,
                        "ATS content",
                        null,
                        warnings,
                        cancellationToken)),
                CreateSectionWork(
                    SpellingAndGrammarSection,
                    "spelling and grammar",
                    RunStreamTaskAsync(
                        aiModel,
                        fileId,
                        _spellingTask,
                        "spelling and grammar",
                        null,
                        warnings,
                        cancellationToken)),
                CreateSectionWork(
                    JobSearchProfileSection,
                    "job search profile",
                    RunStreamTaskAsync(
                        aiModel,
                        fileId,
                        _jobSearchProfileTask,
                        "job search profile",
                        null,
                        warnings,
                        cancellationToken))
            };

            foreach (var sectionTask in sectionTasks)
            {
                yield return CreateEvent("section_started", sectionTask.Section, null, null, completedSections);
            }

            while (sectionTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(sectionTasks.Select(section => section.Task));
                var section = sectionTasks.Single(sectionTask => sectionTask.Task == completedTask);
                sectionTasks.Remove(section);

                var result = await completedTask;
                if (result.Success)
                {
                    ApplySection(response, result.Section, result.Payload);
                    completedSections.Add(result.Section);

                    yield return CreateEvent(
                        "section_completed",
                        result.Section,
                        result.Payload,
                        null,
                        completedSections);
                }
                else
                {
                    yield return CreateEvent(
                        "section_failed",
                        result.Section,
                        result.Payload,
                        result.Warning,
                        completedSections);
                }
            }

            response.Warnings = warnings.ToList();

            yield return CreateEvent("review_completed", null, response, null, completedSections);
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

    private async Task<SectionCompletion> RunStreamTaskAsync<T>(
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
            var payload = await _aiProvider.SendStructuredRequestAsync<T>(
                aiModel,
                fileId,
                task.BuildPrompt(jobDescription),
                task.SchemaName,
                task.Schema,
                cancellationToken);

            return SectionCompletion.Succeeded(task.SchemaName, payload!);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streamed resume analysis section {SectionName} failed.", sectionName);
            var warning = $"{sectionName} could not be generated. Other report sections may still be usable.";
            warnings.Add(warning);

            return SectionCompletion.Failed(task.SchemaName, new T(), warning);
        }
    }

    private static SectionWork CreateSectionWork(
        string section,
        string displayName,
        Task<SectionCompletion> task)
    {
        return new SectionWork(section, displayName, task);
    }

    private static ResumeReviewStreamEnvelope CreateEvent(
        string eventName,
        string? section,
        object? payload,
        string? warning,
        List<string> completedSections)
    {
        return new ResumeReviewStreamEnvelope
        {
            EventName = eventName,
            Data = new ResumeReviewStreamEvent
            {
                Section = section,
                Payload = payload,
                Warning = warning,
                CompletedSections = completedSections.ToList(),
                Timestamp = DateTimeOffset.UtcNow
            }
        };
    }

    private static void ApplySection(ResumeReviewResponse response, string section, object payload)
    {
        switch (section)
        {
            case JobRecommendationSection:
                response.JobRecommendation = (JobRecommendation)payload;
                break;
            case AtsContentSection:
                response.AtsContent = (AtsContent)payload;
                break;
            case SpellingAndGrammarSection:
                response.SpellingAndGrammar = (SpellingAndGrammar)payload;
                break;
            case JobSearchProfileSection:
                response.JobSearchProfile = (JobSearchProfile)payload;
                break;
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

    private sealed record SectionWork(string Section, string DisplayName, Task<SectionCompletion> Task);

    private sealed record SectionCompletion(
        string SchemaName,
        bool Success,
        object Payload,
        string? Warning)
    {
        public string Section => SchemaName switch
        {
            "job_recommendation" => JobRecommendationSection,
            "ats_content" => AtsContentSection,
            "spelling_and_grammar" => SpellingAndGrammarSection,
            "job_search_profile" => JobSearchProfileSection,
            _ => SchemaName
        };

        public static SectionCompletion Succeeded(string schemaName, object payload)
        {
            return new SectionCompletion(schemaName, true, payload, null);
        }

        public static SectionCompletion Failed(string schemaName, object payload, string warning)
        {
            return new SectionCompletion(schemaName, false, payload, warning);
        }
    }
}
