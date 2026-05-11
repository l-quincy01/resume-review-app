using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.ContextualKeywordScoring;

public interface IKeyWordAnalysisService
{
    Task<KeywordAnalysisResponse> ScoreKeywordsAsync(
        string aiModel,
        KeywordExtractionResponse keywords,
        string resumeText,
        CancellationToken cancellationToken = default);
}

