
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Models;

namespace ResumeReview.Api.Services;

public interface IAiResumeAnalysisService
{
    Task<ResumeReviewResponse> AnalyzeResumeAsync(
            string aiModel,
        Stream pdfStream,
        string fileName,
        string? contentType,
        string? jobDescription,
        CancellationToken cancellationToken = default
    );


}