using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services;

namespace ResumeReview.Api.Services.ResumeReview;

public class ResumeReviewService : IResumeReviewService
{
    private readonly IAiResumeAnalysisService _aiResumeAnalysisService;

    public ResumeReviewService(IAiResumeAnalysisService aiResumeAnalysisService)
    {
        _aiResumeAnalysisService = aiResumeAnalysisService;
    }

    public async Task<ResumeReviewResponse> AnalyzeAsync(
        string apiKey,
        ResumeReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var stream = request.Resume.OpenReadStream();

        return await _aiResumeAnalysisService.AnalyzeResumeAsync(
            apiKey: apiKey,
            aiModel: request.AiModel,
            pdfStream: stream,
            fileName: request.Resume.FileName,
            contentType: request.Resume.ContentType,
            jobDescription: request.JobDescription,
            cancellationToken: cancellationToken);
    }

    public async IAsyncEnumerable<ResumeReviewStreamEnvelope> AnalyzeStreamAsync(
        string apiKey,
        ResumeReviewRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var stream = request.Resume.OpenReadStream();

        await foreach (var streamEvent in _aiResumeAnalysisService.AnalyzeResumeStreamAsync(
                           apiKey: apiKey,
                           aiModel: request.AiModel,
                           pdfStream: stream,
                           fileName: request.Resume.FileName,
                           contentType: request.Resume.ContentType,
                           jobDescription: request.JobDescription,
                           cancellationToken: cancellationToken))
        {
            yield return streamEvent;
        }
    }
}
