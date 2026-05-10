using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsEngine;

public interface IContextualKeywordScoringService
{
    Task<ContextualKeywordScoringResponse> ScoreKeywordsAsync(
        string aiModel,
        KeywordExtractionResponse keywords,
        string resumeText,
        CancellationToken cancellationToken = default);
}
