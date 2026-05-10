using ResumeReview.Api.Dtos.Responses;
using ResumeReview.Api.Services.AtsEngine;

namespace ResumeReview.Api.Tests;

public class KeywordExtractionEnricherTests
{
    [Theory]
    [InlineData("React", "single_word")]
    [InlineData("Single Page Applications", "multi_word")]
    [InlineData("problem-solving", "single_word")]
    public void GetKeywordType_ClassifiesBySpacesOnly(string keyword, string expectedType)
    {
        Assert.Equal(expectedType, KeywordExtractionEnricher.GetKeywordType(keyword));
    }

    [Fact]
    public void CountOccurrences_CountsKeywordAndVariationsCaseInsensitively()
    {
        var count = KeywordExtractionEnricher.CountOccurrences(
            "Build SPA experiences with single page applications. SPA experience matters.",
            "Single Page Applications",
            ["SPA"]);

        Assert.Equal(3, count);
    }

    [Fact]
    public void CountOccurrences_UsesWordBoundaries()
    {
        var count = KeywordExtractionEnricher.CountOccurrences(
            "React developers build UI. Reactive programming is separate.",
            "React");

        Assert.Equal(1, count);
    }

    [Fact]
    public void Enrich_BoostsEligibleCategories()
    {
        var response = CreateResponse(new KeywordExtractionItemResponse
        {
            Keyword = "React",
            Category = "framework",
            Tier = 2,
            Requirement = "must_have",
            Context = "Used for frontend UI."
        });

        var result = CreateEnricher().Enrich(response, "React engineers build React interfaces.");
        var keyword = Assert.Single(result.Keywords);

        Assert.Equal("single_word", keyword.KeywordType);
        Assert.Equal(2, keyword.Frequency);
        Assert.Equal(1, keyword.Tier);
        Assert.True(keyword.BoostApplied);
    }

    [Fact]
    public void Enrich_DoesNotBoostWeakGenericTerms()
    {
        var response = CreateResponse(new KeywordExtractionItemResponse
        {
            Keyword = "communication",
            Category = "soft_skill",
            Tier = 2,
            Requirement = "must_have",
            Context = "Used for team communication."
        });

        var result = CreateEnricher().Enrich(response, "Communication and communication skills are important.");

        Assert.Empty(result.Keywords);
    }

    [Fact]
    public void Enrich_DoesNotBoostIneligibleCategories()
    {
        var response = CreateResponse(new KeywordExtractionItemResponse
        {
            Keyword = "stakeholder",
            Category = "generic_business_term",
            Tier = 2,
            Requirement = "must_have",
            Context = "Used for stakeholder alignment."
        });

        var result = CreateEnricher().Enrich(response, "Stakeholder work with stakeholder groups.");

        Assert.Empty(result.Keywords);
    }

    [Fact]
    public void Enrich_DropsRequirementAndSoftSkillPhrases()
    {
        var response = CreateResponse(
            new KeywordExtractionItemResponse
            {
                Keyword = "0+ years Java programming experience",
                Category = "role_specific_requirement",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Postgraduate degree (Computer Science / IT) or equivalent experience",
                Category = "role_specific_requirement",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Reliability / Can-do attitude",
                Category = "soft_skill",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Communication and collaboration",
                Category = "soft_skill",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Java",
                Category = "language",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Spring",
                Category = "framework",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "TDD",
                Category = "methodology",
                Tier = 1,
                Requirement = "must_have",
                Variations = ["Test-Driven Development"]
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Unit Testing",
                Category = "methodology",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Agile",
                Category = "methodology",
                Tier = 2,
                Requirement = "nice_to_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Eclipse",
                Category = "tool",
                Tier = 2,
                Requirement = "nice_to_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "IntelliJ",
                Category = "tool",
                Tier = 2,
                Requirement = "nice_to_have"
            });

        var result = CreateEnricher().Enrich(
            response,
            "Java Spring TDD Unit Testing Agile Eclipse IntelliJ");
        var keywords = result.Keywords.Select(keyword => keyword.Keyword).ToArray();

        Assert.DoesNotContain("0+ years Java programming experience", keywords);
        Assert.DoesNotContain("Postgraduate degree (Computer Science / IT) or equivalent experience", keywords);
        Assert.DoesNotContain("Reliability / Can-do attitude", keywords);
        Assert.DoesNotContain("Communication and collaboration", keywords);
        Assert.Equal(["Java", "Spring", "TDD", "Unit Testing", "Agile", "Eclipse", "IntelliJ"], keywords);
    }

    [Fact]
    public void Enrich_DropsDisallowedCategories()
    {
        var response = CreateResponse(
            new KeywordExtractionItemResponse
            {
                Keyword = "Java",
                Category = "language",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Documentation",
                Category = "role_specific_requirement",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Teamwork",
                Category = "soft_skill",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Business",
                Category = "generic_business_term",
                Tier = 1,
                Requirement = "must_have"
            },
            new KeywordExtractionItemResponse
            {
                Keyword = "Learning resources",
                Category = "other",
                Tier = 2,
                Requirement = "nice_to_have"
            });

        var result = CreateEnricher().Enrich(response, "Java documentation teamwork business resources");

        var keyword = Assert.Single(result.Keywords);
        Assert.Equal("Java", keyword.Keyword);
    }

    private static KeywordExtractionEnricher CreateEnricher()
    {
        return new KeywordExtractionEnricher();
    }

    private static KeywordExtractionResponse CreateResponse(params KeywordExtractionItemResponse[] keywords)
    {
        return new KeywordExtractionResponse
        {
            JobTitle = "Frontend Developer",
            Keywords = keywords.ToList()
        };
    }
}
