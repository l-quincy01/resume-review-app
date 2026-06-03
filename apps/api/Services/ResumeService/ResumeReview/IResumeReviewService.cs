using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Dtos.Requests;

namespace ResumeReview.Api.Services;

public interface IResumeReviewService
{
    Task<ResumeReviewResponse> AnalyzeAsync(
        string apiKey,
        ResumeReviewRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<ResumeReviewStreamEnvelope> AnalyzeStreamAsync(
        string apiKey,
        ResumeReviewRequest request,
        CancellationToken cancellationToken = default);
}
