using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.KeywordExtraction;

public interface IKeywordExtractionService
{
    Task<KeywordExtractionResponse> ExtractKeywordsAsync(
        string aiModel,
        string jobDescription,
        CancellationToken cancellationToken = default);
}
