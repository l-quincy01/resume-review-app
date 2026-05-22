using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Dtos.Requests;

namespace ResumeReview.Api.Services;

public interface IResumeReviewService
{
    Task<ResumeReviewResponse> AnalyzeAsync(
        ResumeReviewRequest request,
        CancellationToken cancellationToken = default);
}