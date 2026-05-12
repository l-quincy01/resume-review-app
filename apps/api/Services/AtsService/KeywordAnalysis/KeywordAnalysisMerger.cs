using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.KeywordAnalysis;

public sealed class KeywordAnalysisMerger
{
    private const int MaxEvidenceSnippets = 3;

    public KeywordAnalysisResponse Merge(KeywordExtractionResponse stageBKeywords, KeywordAnalysisResponse llmResponse)
    {
        var llmScores = llmResponse.KeywordScores
            .GroupBy(score => score.Keyword, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return new KeywordAnalysisResponse
        {
            KeywordScores = stageBKeywords.Keywords
                .Select(keyword =>
                {
                    var score = llmScores.TryGetValue(keyword.Keyword, out var llmScore)
                        ? llmScore
                        : CreateMissingScore(keyword.Keyword);

                    return InjectMetadata(keyword, score);
                })
                .ToList()
        };
    }

    private static KeywordAnalysisItemResponse InjectMetadata(
        KeywordExtractionItemResponse keyword,
        KeywordAnalysisItemResponse score)
    {
        return new KeywordAnalysisItemResponse
        {
            Keyword = keyword.Keyword,
            Present = score.Present,
            Tier = keyword.Tier,
            Requirement = keyword.Requirement,
            KeywordType = keyword.KeywordType,
            Frequency = keyword.Frequency,
            Context = keyword.Context,
            Variations = keyword.Variations,
            MatchedTerms = score.MatchedTerms,
            ContextType = score.ContextType ?? new KeywordContextTypeResponse(),
            Evidence = score.Evidence.Take(MaxEvidenceSnippets).ToList()
        };
    }

    private static KeywordAnalysisItemResponse CreateMissingScore(string keyword)
    {
        return new KeywordAnalysisItemResponse
        {
            Keyword = keyword,
            Present = false,
            MatchedTerms = [],
            ContextType = new KeywordContextTypeResponse(),
            Evidence = []
        };
    }
}
