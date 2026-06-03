
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Models;

namespace ResumeReview.Api.Services;

public interface IAiResumeAnalysisService
{
    Task<ResumeReviewResponse> AnalyzeResumeAsync(
        string apiKey,
        string aiModel,
        Stream pdfStream,
        string fileName,
        string? contentType,
        string? jobDescription,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<ResumeReviewStreamEnvelope> AnalyzeResumeStreamAsync(
        string apiKey,
        string aiModel,
        Stream pdfStream,
        string fileName,
        string? contentType,
        string? jobDescription,
        CancellationToken cancellationToken = default);
}
