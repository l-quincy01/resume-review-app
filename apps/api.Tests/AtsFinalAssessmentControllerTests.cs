using Microsoft.AspNetCore.Mvc;
using ResumeReview.Api.Controllers;
using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services.AtsService.ContextualKeywordScoring;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.TextExtraction;

namespace ResumeReview.Api.Tests;

public class AtsValidatorControllerTests
{
    [Fact]
    public void Assess_RejectsMissingBody()
    {
        var controller = CreateController();

        var result = controller.Assess(null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Request body is required", badRequest.Value!.ToString());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Assess_RejectsInvalidHeaderQualityScore(int headerScore)
    {
        var controller = CreateController();

        var result = controller.Assess(CreateRequest(headerScore, Keyword()));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("header_quality_score", badRequest.Value!.ToString());
    }

    [Fact]
    public void Assess_RejectsEmptyKeywordScores()
    {
        var controller = CreateController();

        var result = controller.Assess(new FinalAssessmentRequest
        {
            HeaderQualityScore = 90
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("at least one keyword", badRequest.Value!.ToString());
    }

    [Fact]
    public void Assess_ReturnsFinalAssessment()
    {
        var controller = CreateController();

        var result = controller.Assess(CreateRequest(90, Keyword()));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<FinalAssessmentResponse>(ok.Value);
        Assert.Equal("Frontend Developer", response.JobTitle);
        Assert.Equal(100, response.OverallKeywordScore);
        Assert.Equal(97, response.AtsReadinessScore);
        Assert.Equal(90, response.HeaderQualityScore);
        Assert.Equal(100, response.Coverage.OverallPresenceRate);
    }

    private static AtsValidatorController CreateController()
    {
        return new AtsValidatorController(new FinalAssessmentService());
    }

    private static FinalAssessmentRequest CreateRequest(int headerScore, params ScoredKeywordResponse[] keywords)
    {
        return new FinalAssessmentRequest
        {
            JobTitle = "Frontend Developer",
            HeaderQualityScore = headerScore,
            KeywordScores = keywords.ToList()
        };
    }

    private static ScoredKeywordResponse Keyword()
    {
        return new ScoredKeywordResponse
        {
            Keyword = "React",
            Present = true,
            Tier = 0,
            Requirement = "must_have",
            KeywordType = "single_word",
            Frequency = 1,
            Context = "Used to build frontend interfaces.",
            Variations = [],
            MatchedTerms = ["React"],
            ContextType = new KeywordContextTypeResponse
            {
                HasAchievement = true,
                InExperienceSection = true
            },
            Evidence = [],
            RequirementMultiplier = 1.15m,
            ContextPoints = 40,
            KeywordScore = 100
        };
    }
}
