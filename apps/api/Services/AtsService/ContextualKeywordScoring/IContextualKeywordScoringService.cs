using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.ContextualKeywordScoring;

public interface IContextualKeywordScoringService
{
    Task<ContextualKeywordScoringResponse> ScoreKeywordsAsync(
        string aiModel,
        KeywordExtractionResponse keywords,
        string resumeText,
        CancellationToken cancellationToken = default);
}
