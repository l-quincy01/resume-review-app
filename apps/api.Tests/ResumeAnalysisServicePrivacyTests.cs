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
            ThrowOnSchemaName = "job_match"
        };
        var service = CreateService(provider);

        var response = await service.AnalyzeResumeAsync(
            "gpt-4.1-mini",
            new MemoryStream([1, 2, 3]),
            "resume.pdf",
            "application/pdf",
            "Build APIs",
            CancellationToken.None);

        Assert.Equal("file-test", provider.DeletedFileIds.Single());
        Assert.Contains(response.Warnings, warning => warning.Contains("job match"));
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

    private static ResumeAnalysisService CreateService(
        IAiProviderClient provider,
        ILogger<ResumeAnalysisService>? logger = null)
    {
        return new ResumeAnalysisService(
            provider,
            logger ?? new ListLogger<ResumeAnalysisService>(),
            new JobRecommendationTask(),
            new JobMatchTask(),
            new AtsContentTask(),
            new SpellingAndGrammarTask(),
            new JobSearchProfileTask());
    }

    private sealed class RecordingAiProviderClient : IAiProviderClient
    {
        public string? ThrowOnSchemaName { get; set; }
        public bool ThrowOnDelete { get; set; }
        public List<string> DeletedFileIds { get; } = [];

        public Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult("file-test");
        }

        public Task DeleteFileAsync(
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
