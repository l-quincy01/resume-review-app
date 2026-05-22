using Microsoft.AspNetCore.Mvc;
using ResumeReview.Api.Controllers;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services.AtsService.KeywordAnalysis;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Tests;

public class KeywordScoringControllerTests
{
    [Fact]
    public void ScoreKeywords_RejectsMissingBody()
    {
        var controller = CreateController();

        var result = controller.ScoreKeywords(null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Request body is required", badRequest.Value!.ToString());
    }

    [Fact]
    public void ScoreKeywords_RejectsEmptyKeywordScores()
    {
        var controller = CreateController();

        var result = controller.ScoreKeywords(new KeywordScoringRequest());

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("at least one keyword", badRequest.Value!.ToString());
    }

    [Fact]
    public void ScoreKeywords_RejectsNullKeywordScores()
    {
        var controller = CreateController();

        var result = controller.ScoreKeywords(new KeywordScoringRequest
        {
            KeywordScores = null!
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("keyword_scores is required", badRequest.Value!.ToString());
    }

    [Fact]
    public void ScoreKeywords_RejectsNullKeywordItem()
    {
        var controller = CreateController();
        var request = new KeywordScoringRequest
        {
            KeywordScores = [null!]
        };

        var result = controller.ScoreKeywords(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("keyword_scores[0] is required", badRequest.Value!.ToString());
    }

    [Fact]
    public void ScoreKeywords_RejectsInvalidTier()
    {
        var controller = CreateController();
        var request = CreateRequest(CreateKeyword(tier: 4));

        var result = controller.ScoreKeywords(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("tier must be", badRequest.Value!.ToString());
    }

    [Fact]
    public void ScoreKeywords_RejectsInvalidRequirement()
    {
        var controller = CreateController();
        var request = CreateRequest(CreateKeyword(requirement: "critical"));

        var result = controller.ScoreKeywords(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("requirement must be", badRequest.Value!.ToString());
    }

    [Fact]
    public void ScoreKeywords_ReturnsScoredKeywordResponse()
    {
        var controller = CreateController();
        var request = CreateRequest(CreateKeyword());

        var result = controller.ScoreKeywords(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<KeywordScoringResponse>(ok.Value);
        var score = Assert.Single(response.KeywordScores);
        Assert.Equal("React", score.Keyword);
        Assert.Equal(1.15m, score.RequirementMultiplier);
        Assert.Equal(50, score.ContextPoints);
        Assert.Equal(58, score.KeywordScore);
    }

    private static KeywordScoringController CreateController()
    {
        return new KeywordScoringController(new KeywordScoringService());
    }

    private static KeywordScoringRequest CreateRequest(params KeywordAnalysisItemResponse[] keywords)
    {
        return new KeywordScoringRequest
        {
            KeywordScores = keywords.ToList()
        };
    }

    private static KeywordAnalysisItemResponse CreateKeyword(
        int tier = 0,
        string requirement = "must_have")
    {
        return new KeywordAnalysisItemResponse
        {
            Keyword = "React",
            Present = true,
            Tier = tier,
            Requirement = requirement,
            KeywordType = "single_word",
            Frequency = 1,
            Context = "Used to build UI.",
            MatchedTerms = ["React"],
            ContextType = new KeywordContextTypeResponse
            {
                HasAchievement = true
            },
            Evidence = []
        };
    }
}
