using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.KeywordScoring;

public interface IKeywordScoringService
{
    KeywordScoringResponse Score(ContextualKeywordScoringResponse contextualKeywords);
}
