using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services.AtsService.ContextualKeywordScoring;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Tests;

public class KeywordAnalysisMergerTests
{
    [Fact]
    public void Merge_InjectsStageBMetadataIntoLlmResults()
    {
        var stageB = CreateStageB();
        var llm = new KeywordAnalysisResponse
        {
            KeywordScores =
            [
                new KeywordAnalysisObject
                {
                    Keyword = "React",
                    Present = true,
                    MatchedTerms = ["React"],
                    ContextType = new KeywordContextTypeResponse
                    {
                        HasAchievement = true,
                        HasMetric = true,
                        HasActionVerb = true,
                        InExperienceSection = true
                    },
                    Evidence =
                    [
                        new KeywordEvidenceResponse
                        {
                            Section = "Work Experience",
                            Text = "Developed React dashboards that reduced reporting time by 35%.",
                            MatchedTerm = "React"
                        }
                    ]
                }
            ]
        };

        var result = CreateMerger().Merge(stageB, llm);
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal("React", score.Keyword);
        Assert.True(score.Present);
        Assert.Equal(0, score.Tier);
        Assert.Equal("must_have", score.Requirement);
        Assert.Equal("single_word", score.KeywordType);
        Assert.Equal(4, score.Frequency);
        Assert.Equal("Used to build frontend interfaces.", score.Context);
        Assert.Equal(["React.js"], score.Variations);
        Assert.True(score.ContextType.HasAchievement);
    }

    [Fact]
    public void Merge_FillsMissingLlmKeywordsAsNotPresent()
    {
        var result = CreateMerger().Merge(CreateStageB(), new KeywordAnalysisResponse());
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal("React", score.Keyword);
        Assert.False(score.Present);
        Assert.Empty(score.MatchedTerms);
        Assert.Empty(score.Evidence);
        Assert.False(score.ContextType.HasAchievement);
        Assert.False(score.ContextType.HasMetric);
        Assert.False(score.ContextType.HasActionVerb);
        Assert.False(score.ContextType.InExperienceSection);
        Assert.False(score.ContextType.InProjectSection);
        Assert.False(score.ContextType.InSummarySection);
    }

    [Fact]
    public void Merge_CapsEvidenceAtThreeSnippets()
    {
        var stageB = CreateStageB();
        var llm = new KeywordAnalysisResponse
        {
            KeywordScores =
            [
                new KeywordAnalysisObject
                {
                    Keyword = "React",
                    Present = true,
                    ContextType = new KeywordContextTypeResponse(),
                    Evidence =
                    [
                        CreateEvidence("one"),
                        CreateEvidence("two"),
                        CreateEvidence("three"),
                        CreateEvidence("four")
                    ]
                }
            ]
        };

        var result = CreateMerger().Merge(stageB, llm);
        var score = Assert.Single(result.KeywordScores);

        Assert.Equal(3, score.Evidence.Count);
        Assert.Equal("one", score.Evidence[0].Text);
        Assert.Equal("three", score.Evidence[2].Text);
    }

    private static KeywordAnalysisMerger CreateMerger()
    {
        return new KeywordAnalysisMerger();
    }

    private static KeywordExtractionResponse CreateStageB()
    {
        return new KeywordExtractionResponse
        {
            JobTitle = "Frontend Developer",
            Keywords =
            [
                new KeywordExtractionItemResponse
                {
                    Keyword = "React",
                    KeywordType = "single_word",
                    Category = "framework",
                    Tier = 0,
                    Requirement = "must_have",
                    Context = "Used to build frontend interfaces.",
                    Variations = ["React.js"],
                    Frequency = 4,
                    BoostApplied = true
                }
            ]
        };
    }

    private static KeywordEvidenceResponse CreateEvidence(string text)
    {
        return new KeywordEvidenceResponse
        {
            Section = "Work Experience",
            Text = text,
            MatchedTerm = "React"
        };
    }
}
