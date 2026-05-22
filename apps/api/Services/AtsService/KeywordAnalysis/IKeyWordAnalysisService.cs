using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.KeywordAnalysis;

public interface IKeywordAnalysisService
{
    Task<KeywordAnalysisResponse> ScoreKeywordsAsync(
        string aiModel,
        KeywordExtractionResponse keywords,
        string resumeText,
        CancellationToken cancellationToken = default);
}

