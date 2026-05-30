using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Models;
using ResumeReview.Api.Services;
using ResumeReview.Api.Services.Providers;
using ResumeReview.Api.Services.Tasks;

namespace ResumeReview.Api.Tests;

public class ResumeAnalysisServicePrivacyTests
{
    [Fact]
    public async Task AnalyzeResumeAsync_DeletesUploadedFileAfterSuccessfulAnalysis()
    {
        var provider = new RecordingAiProviderClient();
        var service = CreateService(provider);

        await service.AnalyzeResumeAsync(
            "user-test-key",
            "gpt-4.1-mini",
            new MemoryStream([1, 2, 3]),
            "resume.pdf",
            "application/pdf",
            "Build APIs",
            CancellationToken.None);

        Assert.Equal("file-test", provider.DeletedFileIds.Single());
    }

    [Fact]
    public async Task AnalyzeResumeAsync_DeletesUploadedFileWhenAnalysisSectionsFail()
    {
        var provider = new RecordingAiProviderClient
        {
            ThrowOnSchemaName = "ats_content"
        };
        var service = CreateService(provider);

        var response = await service.AnalyzeResumeAsync(
            "user-test-key",
            "gpt-4.1-mini",
            new MemoryStream([1, 2, 3]),
            "resume.pdf",
            "application/pdf",
            "Build APIs",
            CancellationToken.None);

        Assert.Equal("file-test", provider.DeletedFileIds.Single());
        Assert.Contains(response.Warnings, warning => warning.Contains("ATS content"));
    }

    [Fact]
    public async Task AnalyzeResumeAsync_DeleteFailureDoesNotHideAnalysisResult()
    {
        var provider = new RecordingAiProviderClient
        {
            ThrowOnDelete = true
        };
        var logger = new ListLogger<ResumeAnalysisService>();
        var service = CreateService(provider, logger);

        var response = await service.AnalyzeResumeAsync(
            "user-test-key",
            "gpt-4.1-mini",
            new MemoryStream([1, 2, 3]),
            "resume.pdf",
            "application/pdf",
            "Build APIs",
            CancellationToken.None);

        Assert.Equal("file-test", provider.DeletedFileIds.Single());
        Assert.NotNull(response.JobRecommendation);
        Assert.Contains(logger.Entries, entry => entry.Message.Contains("OpenAI file cleanup failed"));
    }

    [Fact]
    public async Task AnalyzeResumeStreamAsync_EmitsSectionEventsAndDeletesUploadedFile()
    {
        var provider = new RecordingAiProviderClient();
        var service = CreateService(provider);

        var events = await CollectEventsAsync(service.AnalyzeResumeStreamAsync(
            "user-test-key",
            "gpt-4.1-mini",
            new MemoryStream([1, 2, 3]),
            "resume.pdf",
            "application/pdf",
            "Build APIs",
            CancellationToken.None));

        Assert.Equal("review_started", events.First().EventName);
        Assert.Contains(events, streamEvent =>
            streamEvent.EventName == "section_started" &&
            streamEvent.Data.Section == "ats_content");
        Assert.Contains(events, streamEvent =>
            streamEvent.EventName == "section_completed" &&
            streamEvent.Data.Section == "ats_content");
        Assert.Equal("review_completed", events.Last().EventName);
        Assert.IsType<ResumeReviewResponse>(events.Last().Data.Payload);
        Assert.Equal("file-test", provider.DeletedFileIds.Single());
    }

    [Fact]
    public async Task AnalyzeResumeStreamAsync_EmitsSectionFailedAndContinues()
    {
        var provider = new RecordingAiProviderClient
        {
            ThrowOnSchemaName = "ats_content"
        };
        var service = CreateService(provider);

        var events = await CollectEventsAsync(service.AnalyzeResumeStreamAsync(
            "user-test-key",
            "gpt-4.1-mini",
            new MemoryStream([1, 2, 3]),
            "resume.pdf",
            "application/pdf",
            "Build APIs",
            CancellationToken.None));

        Assert.Contains(events, streamEvent =>
            streamEvent.EventName == "section_failed" &&
            streamEvent.Data.Section == "ats_content" &&
            streamEvent.Data.Warning!.Contains("ATS content"));
        Assert.Contains(events, streamEvent =>
            streamEvent.EventName == "section_completed" &&
            streamEvent.Data.Section == "spelling_and_grammar");
        Assert.Equal("review_completed", events.Last().EventName);
    }

    private static ResumeAnalysisService CreateService(
        IAiProviderClient provider,
        ILogger<ResumeAnalysisService>? logger = null)
    {
        return new ResumeAnalysisService(
            provider,
            logger ?? new ListLogger<ResumeAnalysisService>(),
            new JobRecommendationTask(),
            new AtsContentTask(),
            new SpellingAndGrammarTask(),
            new JobSearchProfileTask());
    }

    private static async Task<List<ResumeReviewStreamEnvelope>> CollectEventsAsync(
        IAsyncEnumerable<ResumeReviewStreamEnvelope> events)
    {
        var collected = new List<ResumeReviewStreamEnvelope>();

        await foreach (var streamEvent in events)
        {
            collected.Add(streamEvent);
        }

        return collected;
    }

    private sealed class RecordingAiProviderClient : IAiProviderClient
    {
        public string? ThrowOnSchemaName { get; set; }
        public bool ThrowOnDelete { get; set; }
        public List<string> DeletedFileIds { get; } = [];

        public Task<string> UploadFileAsync(
            string apiKey,
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult("file-test");
        }

        public Task DeleteFileAsync(
            string apiKey,
            string fileId,
            CancellationToken cancellationToken = default)
        {
            DeletedFileIds.Add(fileId);

            if (ThrowOnDelete)
            {
                throw new InvalidOperationException("cleanup failed");
            }

            return Task.CompletedTask;
        }

        public Task<T> SendStructuredRequestAsync<T>(
            string apiKey,
            string model,
            string fileId,
            string prompt,
            string schemaName,
            object schema,
            CancellationToken cancellationToken = default)
        {
            if (schemaName == ThrowOnSchemaName)
            {
                throw new InvalidOperationException("analysis failed");
            }

            return Task.FromResult((T)Activator.CreateInstance(typeof(T))!);
        }
    }
}
