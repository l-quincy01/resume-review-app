using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services.AtsService.KeywordAnalysis;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Tests;

public class FinalAssessmentServiceTests
{
    [Fact]
    public void Assess_IncludesMissingKeywordsAsZeroInTierAverages()
    {
        var result = CreateService().Assess(CreateRequest(
            headerScore: 90,
            Keyword("React", tier: 0, present: true, score: 100),
            Keyword("TypeScript", tier: 0, present: false, score: 100)));

        Assert.Equal(50, result.TierBreakdown.Tier0.AverageScore);
        Assert.Equal(["TypeScript"], result.TierBreakdown.Tier0.Missing);
    }

    [Fact]
    public void Assess_RedistributesWeightsWhenTiersAreAbsent()
    {
        var result = CreateService().Assess(CreateRequest(
            headerScore: 0,
            Keyword("React", tier: 1, present: true, score: 80)));

        Assert.Equal(80, result.OverallKeywordScore);
        Assert.Equal(56, result.AtsReadinessScore);
    }

    [Fact]
    public void Assess_CalculatesCoverageMetrics()
    {
        var result = CreateService().Assess(CreateRequest(
            headerScore: 90,
            Keyword("React", tier: 0, present: true, score: 100),
            Keyword("TypeScript", tier: 0, present: false, score: 0),
            Keyword("REST APIs", tier: 1, present: true, score: 80),
            Keyword("Testing", tier: 2, present: false, score: 0)));

        Assert.Equal(50, result.Coverage.OverallPresenceRate);
        Assert.Equal(50, result.Coverage.Tier0Coverage);
        Assert.Equal(100, result.Coverage.Tier1Coverage);
        Assert.Equal(50, result.Coverage.MustHaveCoverage);
        Assert.Equal(2, result.Coverage.TotalKeywordsFound);
        Assert.Equal(4, result.Coverage.TotalKeywordsExpected);
    }

    [Fact]
    public void Assess_CombinesOverallKeywordAndHeaderScores()
    {
        var result = CreateService().Assess(CreateRequest(
            headerScore: 90,
            Keyword("React", tier: 0, present: true, score: 100),
            Keyword("TypeScript", tier: 0, present: false, score: 0),
            Keyword("REST APIs", tier: 1, present: true, score: 80),
            Keyword("Testing", tier: 2, present: false, score: 0)));

        Assert.Equal(49, result.OverallKeywordScore);
        Assert.Equal(61, result.AtsReadinessScore);
    }

    [Fact]
    public void Assess_UsesUpdatedTierWeightsForOverallKeywordScore()
    {
        var result = CreateService().Assess(CreateRequest(
            headerScore: 0,
            Keyword("Tier 0", tier: 0, present: true, score: 100),
            Keyword("Tier 1", tier: 1, present: true, score: 80),
            Keyword("Tier 2", tier: 2, present: true, score: 60),
            Keyword("Tier 3", tier: 3, present: true, score: 40)));

        Assert.Equal(77, result.OverallKeywordScore);
    }

    [Fact]
    public void Assess_CriticalGapsIncludeMissingTierZeroAndOneMustHavesOnly()
    {
        var result = CreateService().Assess(CreateRequest(
            headerScore: 90,
            Keyword("TypeScript", tier: 0, present: false, score: 0),
            Keyword("REST APIs", tier: 1, present: false, score: 0),
            Keyword("Testing", tier: 2, present: false, score: 0),
            Keyword("Figma", tier: 1, present: false, score: 0, requirement: "nice_to_have")));

        Assert.Equal(["TypeScript", "REST APIs"], result.CriticalGaps.Select(gap => gap.Keyword).ToList());
        Assert.All(result.CriticalGaps, gap => Assert.Equal("must_have", gap.Requirement));
        Assert.All(result.CriticalGaps, gap => Assert.False(string.IsNullOrWhiteSpace(gap.KeywordType)));
        Assert.All(result.CriticalGaps, gap => Assert.Equal("Used in the role.", gap.Context));
    }

    [Fact]
    public void Assess_StrengthsIncludeHighScoringPresentKeywordsWithEvidence()
    {
        var result = CreateService().Assess(CreateRequest(
            headerScore: 90,
            Keyword(
                "React",
                tier: 0,
                present: true,
                score: 100,
                contextType: new KeywordContextTypeResponse
                {
                    HasAchievement = true,
                    HasMetric = true,
                    HasActionVerb = true
                },
                evidence: [
                    new KeywordEvidenceResponse { Section = "Work Experience", Text = "Built React dashboards.", MatchedTerm = "React" }
                ]),
            Keyword("CSS", tier: 2, present: true, score: 79)));

        var strength = Assert.Single(result.Strengths);
        Assert.Equal("React", strength.Keyword);
        Assert.Equal("single_word", strength.KeywordType);
        Assert.Equal("Used in the role.", strength.Context);
        Assert.Equal(100, strength.Score);
        Assert.Contains("measurable outcome", strength.Reason);
        Assert.Single(strength.Evidence);
    }

    [Fact]
    public void Assess_RecommendationsUseHighAndMediumPriorityForMissingMustHaves()
    {
        var result = CreateService().Assess(CreateRequest(
            headerScore: 90,
            Keyword("TypeScript", tier: 0, present: false, score: 0),
            Keyword("REST APIs", tier: 1, present: false, score: 0),
            Keyword("Testing", tier: 2, present: false, score: 0),
            Keyword("Figma", tier: 3, present: false, score: 0),
            Keyword("Jira", tier: 1, present: false, score: 0, requirement: "nice_to_have")));

        Assert.Equal(["High", "High", "Medium"], result.Recommendations.Select(recommendation => recommendation.Priority).ToList());
        Assert.Equal(["TypeScript", "REST APIs", "Testing"], result.Recommendations.Select(recommendation => recommendation.Keyword).ToList());
        Assert.All(result.Recommendations, recommendation => Assert.False(string.IsNullOrWhiteSpace(recommendation.KeywordType)));
        Assert.All(result.Recommendations, recommendation => Assert.Equal("Used in the role.", recommendation.Context));
    }

    [Fact]
    public void Assess_EmptyTiersProduceStableZeroCoverageAndMissingArrays()
    {
        var result = CreateService().Assess(CreateRequest(
            headerScore: 90,
            Keyword("React", tier: 0, present: true, score: 100)));

        Assert.Equal(0, result.Coverage.Tier1Coverage);
        Assert.Equal(0, result.TierBreakdown.Tier1.AverageScore);
        Assert.Equal(0, result.TierBreakdown.Tier1.Present);
        Assert.Equal(0, result.TierBreakdown.Tier1.Total);
        Assert.Empty(result.TierBreakdown.Tier1.Missing);
    }

    private static FinalAssessmentService CreateService()
    {
        return new FinalAssessmentService();
    }

    private static FinalAssessmentRequest CreateRequest(int headerScore, params KeywordScoreObject[] keywords)
    {
        return new FinalAssessmentRequest
        {
            JobTitle = "Frontend Developer",
            HeaderQualityScore = headerScore,
            KeywordScores = keywords.ToList()
        };
    }

    private static KeywordScoreObject Keyword(
        string keyword,
        int tier,
        bool present,
        int score,
        string requirement = "must_have",
        KeywordContextTypeResponse? contextType = null,
        List<KeywordEvidenceResponse>? evidence = null)
    {
        return new KeywordScoreObject
        {
            Keyword = keyword,
            Present = present,
            Tier = tier,
            Requirement = requirement,
            KeywordType = keyword.Contains(' ', StringComparison.Ordinal) ? "multi_word" : "single_word",
            Frequency = 1,
            Context = "Used in the role.",
            Variations = [],
            MatchedTerms = present ? [keyword] : [],
            ContextType = contextType ?? new KeywordContextTypeResponse { InExperienceSection = present },
            Evidence = evidence ?? [],
            RequirementMultiplier = string.Equals(requirement, "must_have", StringComparison.OrdinalIgnoreCase) ? 1.15m : 1.05m,
            ContextPoints = present ? score : 0,
            KeywordScore = score
        };
    }
}
