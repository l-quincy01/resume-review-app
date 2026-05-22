using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services.AtsService.KeywordAnalysis;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Tests;

public class KeywordScoringServiceTests
{
    [Fact]
    public void Score_FullTierZeroMustHaveContext_ClampsToOneHundred()
    {
        var result = CreateService().Score(CreateResponse(CreateKeyword(
            tier: 0,
            requirement: "must_have",
            contextType: AllFlags())));
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal(124, score.ContextPoints);
        Assert.Equal(1.15m, score.RequirementMultiplier);
        Assert.Equal(100, score.KeywordScore);
    }

    [Fact]
    public void Score_AppliesTierSpecificPointTables()
    {
        var result = CreateService().Score(CreateResponse(
            CreateKeyword(tier: 1, requirement: "", contextType: new KeywordContextTypeResponse { HasAchievement = true }),
            CreateKeyword(tier: 2, requirement: "", contextType: new KeywordContextTypeResponse { HasMetric = true }),
            CreateKeyword(tier: 3, requirement: "", contextType: new KeywordContextTypeResponse { InProjectSection = true })));

        Assert.Equal(42, result.KeywordScores[0].ContextPoints);
        Assert.Equal(42, result.KeywordScores[0].KeywordScore);
        Assert.Equal(30, result.KeywordScores[1].ContextPoints);
        Assert.Equal(30, result.KeywordScores[1].KeywordScore);
        Assert.Equal(20, result.KeywordScores[2].ContextPoints);
        Assert.Equal(20, result.KeywordScores[2].KeywordScore);
    }

    [Fact]
    public void Score_NiceToHaveUsesOnePointZeroFiveMultiplier()
    {
        var result = CreateService().Score(CreateResponse(CreateKeyword(
            tier: 1,
            requirement: "nice_to_have",
            contextType: new KeywordContextTypeResponse { HasMetric = true })));
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal(1.05m, score.RequirementMultiplier);
        Assert.Equal(39, score.KeywordScore);
    }

    [Fact]
    public void Score_EmptyRequirementUsesOnePointZeroMultiplier()
    {
        var result = CreateService().Score(CreateResponse(CreateKeyword(
            tier: 1,
            requirement: "",
            contextType: new KeywordContextTypeResponse { HasMetric = true })));
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal(1.0m, score.RequirementMultiplier);
        Assert.Equal(37, score.KeywordScore);
    }

    [Fact]
    public void Score_StrongContextWithoutMetricScoresMateriallyHigher()
    {
        var result = CreateService().Score(CreateResponse(CreateKeyword(
            tier: 1,
            requirement: "must_have",
            contextType: new KeywordContextTypeResponse
            {
                HasAchievement = true,
                HasActionVerb = true,
                InExperienceSection = true,
                InProjectSection = true
            })));
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal(70, score.ContextPoints);
        Assert.Equal(81, score.KeywordScore);
    }

    [Fact]
    public void Score_MissingKeywordScoresZero()
    {
        var result = CreateService().Score(CreateResponse(CreateKeyword(
            present: false,
            tier: 0,
            requirement: "must_have",
            contextType: AllFlags())));
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal(0, score.ContextPoints);
        Assert.Equal(1.15m, score.RequirementMultiplier);
        Assert.Equal(0, score.KeywordScore);
    }

    [Fact]
    public void Score_PresentKeywordWithNoContextFlagsUsesStuffingPenalty()
    {
        var result = CreateService().Score(CreateResponse(CreateKeyword(
            tier: 0,
            requirement: "must_have",
            contextType: new KeywordContextTypeResponse())));
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal(-12, score.ContextPoints);
        Assert.Equal(0, score.KeywordScore);
    }

    [Fact]
    public void Score_NormalizesNullableKeywordFields()
    {
        var result = CreateService().Score(CreateResponse(new KeywordAnalysisItemResponse
        {
            Keyword = null!,
            Present = true,
            Tier = 1,
            Requirement = null!,
            KeywordType = null!,
            Frequency = 1,
            Context = null!,
            Variations = null!,
            MatchedTerms = null!,
            ContextType = null!,
            Evidence = null!
        }));
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal(string.Empty, score.Keyword);
        Assert.Equal(string.Empty, score.Requirement);
        Assert.Equal(string.Empty, score.KeywordType);
        Assert.Equal(string.Empty, score.Context);
        Assert.Empty(score.Variations);
        Assert.Empty(score.MatchedTerms);
        Assert.NotNull(score.ContextType);
        Assert.Empty(score.Evidence);
        Assert.Equal(-12, score.ContextPoints);
        Assert.Equal(0, score.KeywordScore);
    }

    private static KeywordScoringService CreateService()
    {
        return new KeywordScoringService();
    }

    private static KeywordAnalysisResponse CreateResponse(params KeywordAnalysisItemResponse[] keywords)
    {
        return new KeywordAnalysisResponse
        {
            KeywordScores = keywords.ToList()
        };
    }

    private static KeywordAnalysisItemResponse CreateKeyword(
        bool present = true,
        int tier = 0,
        string requirement = "must_have",
        KeywordContextTypeResponse? contextType = null)
    {
        return new KeywordAnalysisItemResponse
        {
            Keyword = "React",
            Present = present,
            Tier = tier,
            Requirement = requirement,
            KeywordType = "single_word",
            Frequency = 1,
            Context = "Used to build UI.",
            Variations = [],
            MatchedTerms = present ? ["React"] : [],
            ContextType = contextType ?? new KeywordContextTypeResponse(),
            Evidence = []
        };
    }

    private static KeywordContextTypeResponse AllFlags()
    {
        return new KeywordContextTypeResponse
        {
            HasAchievement = true,
            HasMetric = true,
            HasActionVerb = true,
            InExperienceSection = true,
            InProjectSection = true,
            InSummarySection = true
        };
    }
}
