using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsEngine;

public interface IKeywordExtractionService
{
    Task<KeywordExtractionResponse> ExtractKeywordsAsync(
        string aiModel,
        string jobDescription,
        CancellationToken cancellationToken = default);
}
