using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.KeywordScoring;

public sealed class KeywordScoringService : IKeywordScoringService
{
    private static readonly Dictionary<int, int> PresenceBaselinePoints = new()
    {
        [0] = 25,
        [1] = 22,
        [2] = 18,
        [3] = 15
    };

    private static readonly Dictionary<int, ContextPointTable> PointTables = new()
    {
        [0] = new ContextPointTable(25, 20, 12, 15, 15, 12),
        [1] = new ContextPointTable(20, 15, 8, 10, 10, 10),
        [2] = new ContextPointTable(15, 12, 5, 8, 8, 8),
        [3] = new ContextPointTable(10, 10, 4, 6, 5, 5)
    };

    public KeywordScoringResponse Score(KeywordAnalysisResponse contextualKeywords)
    {
        var keywordScores = contextualKeywords.KeywordScores ?? [];

        return new KeywordScoringResponse
        {
            KeywordScores = keywordScores
                .Where(keyword => keyword is not null)
                .Select(ScoreKeyword)
                .ToList()
        };
    }

    private static KeywordScoreObject ScoreKeyword(KeywordAnalysisItemResponse keyword)
    {
        var contextType = keyword.ContextType ?? new KeywordContextTypeResponse();
        var variations = keyword.Variations ?? [];
        var matchedTerms = keyword.MatchedTerms ?? [];
        var evidence = keyword.Evidence ?? [];
        var multiplier = GetRequirementMultiplier(keyword.Requirement);
        var contextPoints = GetContextPoints(keyword);
        var rawKeywordScore = contextPoints * multiplier;
        var keywordScore = Math.Clamp((int)Math.Round(rawKeywordScore, MidpointRounding.AwayFromZero), 0, 100);

        if (!keyword.Present)
        {
            keywordScore = 0;
        }

        return new KeywordScoreObject
        {
            Keyword = keyword.Keyword ?? string.Empty,
            Present = keyword.Present,
            Tier = keyword.Tier,
            Requirement = keyword.Requirement ?? string.Empty,
            KeywordType = keyword.KeywordType ?? string.Empty,
            Frequency = keyword.Frequency,
            Context = keyword.Context ?? string.Empty,
            Variations = variations,
            MatchedTerms = matchedTerms,
            ContextType = contextType,
            Evidence = evidence,
            RequirementMultiplier = multiplier,
            ContextPoints = contextPoints,
            KeywordScore = keywordScore
        };
    }

    private static int GetContextPoints(KeywordAnalysisItemResponse keyword)
    {
        if (!keyword.Present)
        {
            return 0;
        }

        var contextType = keyword.ContextType ?? new KeywordContextTypeResponse();

        if (!contextType.HasAchievement &&
            !contextType.HasMetric &&
            !contextType.HasActionVerb &&
            !contextType.InExperienceSection &&
            !contextType.InProjectSection &&
            !contextType.InSummarySection)
        {
            return -12;
        }

        var table = PointTables[keyword.Tier];
        var points = PresenceBaselinePoints[keyword.Tier];

        if (contextType.HasAchievement)
        {
            points += table.HasAchievement;
        }

        if (contextType.HasMetric)
        {
            points += table.HasMetric;
        }

        if (contextType.HasActionVerb)
        {
            points += table.HasActionVerb;
        }

        if (contextType.InExperienceSection)
        {
            points += table.InExperienceSection;
        }

        if (contextType.InProjectSection)
        {
            points += table.InProjectSection;
        }

        if (contextType.InSummarySection)
        {
            points += table.InSummarySection;
        }

        return points;
    }

    private static decimal GetRequirementMultiplier(string? requirement)
    {
        return requirement?.Trim().ToLowerInvariant() switch
        {
            "must_have" => 1.15m,
            "nice_to_have" => 1.05m,
            _ => 1.0m
        };
    }

    private sealed record ContextPointTable(
        int HasAchievement,
        int HasMetric,
        int HasActionVerb,
        int InExperienceSection,
        int InProjectSection,
        int InSummarySection);
}
