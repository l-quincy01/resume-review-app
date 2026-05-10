using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsEngine;

public sealed class KeywordScoringService : IKeywordScoringService
{
    private static readonly Dictionary<int, ContextPointTable> PointTables = new()
    {
        [0] = new ContextPointTable(25, 20, 12, 15, 15, 12),
        [1] = new ContextPointTable(20, 15, 8, 10, 10, 10),
        [2] = new ContextPointTable(15, 12, 5, 8, 8, 8),
        [3] = new ContextPointTable(10, 10, 4, 6, 5, 5)
    };

    public KeywordScoringResponse Score(ContextualKeywordScoringResponse contextualKeywords)
    {
        return new KeywordScoringResponse
        {
            KeywordScores = contextualKeywords.KeywordScores
                .Select(ScoreKeyword)
                .ToList()
        };
    }

    private static ScoredKeywordResponse ScoreKeyword(ContextualKeywordScoreResponse keyword)
    {
        var multiplier = GetRequirementMultiplier(keyword.Requirement);
        var contextPoints = GetContextPoints(keyword);
        var rawKeywordScore = contextPoints * multiplier;
        var keywordScore = Math.Clamp((int)Math.Round(rawKeywordScore, MidpointRounding.AwayFromZero), 0, 100);

        if (!keyword.Present)
        {
            keywordScore = 0;
        }

        return new ScoredKeywordResponse
        {
            Keyword = keyword.Keyword,
            Present = keyword.Present,
            Tier = keyword.Tier,
            Requirement = keyword.Requirement,
            KeywordType = keyword.KeywordType,
            Frequency = keyword.Frequency,
            Context = keyword.Context,
            Variations = keyword.Variations,
            MatchedTerms = keyword.MatchedTerms,
            ContextType = keyword.ContextType,
            Evidence = keyword.Evidence,
            RequirementMultiplier = multiplier,
            ContextPoints = contextPoints,
            KeywordScore = keywordScore
        };
    }

    private static int GetContextPoints(ContextualKeywordScoreResponse keyword)
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
        var points = 0;

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
