using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.FinalAssessment;

public sealed class FinalAssessmentService : IFinalAssessmentService
{
    private static readonly Dictionary<int, decimal> TierWeights = new()
    {
        [0] = 0.40m,
        [1] = 0.35m,
        [2] = 0.15m,
        [3] = 0.10m
    };

    public FinalAssessmentResponse Assess(FinalAssessmentRequest request)
    {
        var keywords = request.KeywordScores;
        var tierItems = Enumerable.Range(0, 4)
            .ToDictionary(tier => tier, tier => BuildTierBreakdown(keywords, tier));
        var overallKeywordScore = CalculateOverallKeywordScore(tierItems);
        var atsReadinessScore = ClampScore(Round(
            (overallKeywordScore * 0.70m) + (request.HeaderQualityScore * 0.30m)));

        return new FinalAssessmentResponse
        {
            JobTitle = request.JobTitle ?? string.Empty,
            AtsReadinessScore = atsReadinessScore,
            OverallKeywordScore = overallKeywordScore,
            HeaderQualityScore = request.HeaderQualityScore,
            Coverage = BuildCoverage(keywords),
            TierBreakdown = new FinalAssessmentTierBreakdownResponse
            {
                Tier0 = tierItems[0],
                Tier1 = tierItems[1],
                Tier2 = tierItems[2],
                Tier3 = tierItems[3]
            },
            CriticalGaps = BuildCriticalGaps(keywords),
            Strengths = BuildStrengths(keywords),
            Recommendations = BuildRecommendations(keywords)
        };
    }

    private static FinalAssessmentCoverageResponse BuildCoverage(List<KeywordScoreObject> keywords)
    {
        var total = keywords.Count;
        var present = keywords.Count(keyword => keyword.Present);
        var tier0 = keywords.Where(keyword => keyword.Tier == 0).ToList();
        var tier1 = keywords.Where(keyword => keyword.Tier == 1).ToList();
        var mustHaves = keywords.Where(IsMustHave).ToList();

        return new FinalAssessmentCoverageResponse
        {
            OverallPresenceRate = Percentage(present, total),
            Tier0Coverage = Percentage(tier0.Count(keyword => keyword.Present), tier0.Count),
            Tier1Coverage = Percentage(tier1.Count(keyword => keyword.Present), tier1.Count),
            MustHaveCoverage = Percentage(mustHaves.Count(keyword => keyword.Present), mustHaves.Count),
            TotalKeywordsFound = present,
            TotalKeywordsExpected = total
        };
    }

    private static FinalAssessmentTierBreakdownItemResponse BuildTierBreakdown(
        List<KeywordScoreObject> keywords,
        int tier)
    {
        var tierKeywords = keywords.Where(keyword => keyword.Tier == tier).ToList();

        return new FinalAssessmentTierBreakdownItemResponse
        {
            AverageScore = tierKeywords.Count == 0
                ? 0
                : ClampScore(Round(tierKeywords.Average(EffectiveKeywordScore))),
            Present = tierKeywords.Count(keyword => keyword.Present),
            Total = tierKeywords.Count,
            Missing = tierKeywords
                .Where(keyword => !keyword.Present)
                .Select(keyword => keyword.Keyword)
                .ToList()
        };
    }

    private static int CalculateOverallKeywordScore(Dictionary<int, FinalAssessmentTierBreakdownItemResponse> tierItems)
    {
        var presentTiers = tierItems
            .Where(pair => pair.Value.Total > 0)
            .Select(pair => pair.Key)
            .ToList();

        if (presentTiers.Count == 0)
        {
            return 0;
        }

        var activeWeight = presentTiers.Sum(tier => TierWeights[tier]);
        var weightedScore = presentTiers.Sum(tier =>
            tierItems[tier].AverageScore * (TierWeights[tier] / activeWeight));

        return ClampScore(Round(weightedScore));
    }

    private static List<FinalAssessmentCriticalGapResponse> BuildCriticalGaps(List<KeywordScoreObject> keywords)
    {
        return keywords
            .Where(keyword => !keyword.Present && IsMustHave(keyword) && keyword.Tier is 0 or 1)
            .Select(keyword => new FinalAssessmentCriticalGapResponse
            {
                Keyword = keyword.Keyword,
                Tier = keyword.Tier,
                Requirement = keyword.Requirement,
                Reason = "Missing entirely from the resume despite being a critical must-have requirement."
            })
            .ToList();
    }

    private static List<FinalAssessmentStrengthResponse> BuildStrengths(List<KeywordScoreObject> keywords)
    {
        return keywords
            .Where(keyword => keyword.Present && keyword.KeywordScore >= 80)
            .Select(keyword => new FinalAssessmentStrengthResponse
            {
                Keyword = keyword.Keyword,
                Score = keyword.KeywordScore,
                Reason = BuildStrengthReason(keyword),
                Evidence = keyword.Evidence.Take(3).ToList()
            })
            .ToList();
    }

    private static List<FinalAssessmentRecommendationResponse> BuildRecommendations(List<KeywordScoreObject> keywords)
    {
        return keywords
            .Where(keyword => !keyword.Present && IsMustHave(keyword) && keyword.Tier is 0 or 1 or 2)
            .Select(keyword => new FinalAssessmentRecommendationResponse
            {
                Keyword = keyword.Keyword,
                Priority = keyword.Tier is 0 or 1 ? "High" : "Medium",
                Issue = keyword.Tier is 0 or 1
                    ? "Missing critical must-have keyword."
                    : "Missing must-have keyword.",
                Suggestion = keyword.Tier is 0 or 1
                    ? $"Add {keyword.Keyword} naturally into a recent Work Experience or Projects bullet if it is genuinely part of your experience."
                    : $"Mention {keyword.Keyword} in a relevant experience or project bullet with truthful context if it is genuinely part of your experience."
            })
            .ToList();
    }

    private static string BuildStrengthReason(KeywordScoreObject keyword)
    {
        var context = keyword.ContextType ?? new KeywordContextTypeResponse();

        if (context.HasActionVerb && context.HasAchievement && context.HasMetric)
        {
            return "Well contextualised with an action verb, achievement, and measurable outcome.";
        }

        if (context.InExperienceSection)
        {
            return "Well contextualised in Work Experience with supporting evidence.";
        }

        if (context.InProjectSection)
        {
            return "Well contextualised in Projects with supporting evidence.";
        }

        return "Well contextualised in the resume with supporting evidence.";
    }

    private static bool IsMustHave(KeywordScoreObject keyword)
    {
        return string.Equals(keyword.Requirement, "must_have", StringComparison.OrdinalIgnoreCase);
    }

    private static int EffectiveKeywordScore(KeywordScoreObject keyword)
    {
        return keyword.Present ? keyword.KeywordScore : 0;
    }

    private static int Percentage(int numerator, int denominator)
    {
        return denominator == 0
            ? 0
            : ClampScore(Round((numerator / (decimal)denominator) * 100m));
    }

    private static int Round(decimal value)
    {
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    private static int Round(double value)
    {
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    private static int ClampScore(int score)
    {
        return Math.Clamp(score, 0, 100);
    }
}
