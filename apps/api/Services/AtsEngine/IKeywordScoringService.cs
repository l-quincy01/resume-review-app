using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsEngine;

public interface IKeywordScoringService
{
    KeywordScoringResponse Score(ContextualKeywordScoringResponse contextualKeywords);
}
