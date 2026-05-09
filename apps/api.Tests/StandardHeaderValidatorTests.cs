using ResumeReview.Api.Services.AtsEngine;

namespace ResumeReview.Api.Tests;

public class StandardHeaderValidatorTests
{
    [Fact]
    public void Validate_AllRequiredHeadersFound_ReturnsStrongScore()
    {
        var result = CreateValidator().Validate("""
Summary
Backend developer focused on APIs.
Skills
C#, TypeScript
Work Experience
Built services.
Education
BSc Computer Science
""");

        Assert.Equal(80, result.HeaderQualityScore);
        Assert.Equal("strong", result.StructureQuality);
        Assert.Contains("Summary", result.HeadersFound);
        Assert.Contains("Education", result.HeadersFound);
    }

    [Fact]
    public void Validate_Aliases_NormalizesToCanonicalHeaders()
    {
        var result = CreateValidator().Validate("""
Professional Summary
Technical Skills
Professional Experience
Academic Background
Licences & Certifications
""");

        Assert.Contains("Summary", result.HeadersFound);
        Assert.Contains("Skills", result.HeadersFound);
        Assert.Contains("Work Experience", result.HeadersFound);
        Assert.Contains("Education", result.HeadersFound);
        Assert.Contains("Certifications", result.HeadersFound);
    }

    [Fact]
    public void Validate_WeakAliases_MapToCanonicalHeadersWithReducedCreditAndRecommendation()
    {
        var result = CreateValidator().Validate("""
Profile
Skills
Experiences
Education
""");

        Assert.Equal(70, result.HeaderQualityScore);
        Assert.Contains("Summary", result.HeadersFound);
        Assert.Contains("Work Experience", result.HeadersFound);
        Assert.Contains(result.NonStandardHeaders, header =>
            header.HeaderFound == "Profile" &&
            header.MappedTo == "Summary" &&
            header.RecommendedHeader == "Summary");
        Assert.Contains(result.NonStandardHeaders, header =>
            header.HeaderFound == "Experiences" &&
            header.MappedTo == "Work Experience" &&
            header.RecommendedHeader == "Work Experience");
    }

    [Fact]
    public void Validate_StandardHeaderOverridesWeakAliasRecommendation()
    {
        var result = CreateValidator().Validate("""
Profile
Summary
Skills
Work Experience
Education
""");

        Assert.Equal(80, result.HeaderQualityScore);
        Assert.DoesNotContain(result.NonStandardHeaders, header => header.MappedTo == "Summary");
    }

    [Fact]
    public void Validate_OptionalHeadersAddPointsButMissingOptionalHeadersDoNotReduceRequiredScore()
    {
        var requiredOnly = CreateValidator().Validate("""
Summary
Skills
Work Experience
Education
""");
        var withOptional = CreateValidator().Validate("""
Summary
Skills
Work Experience
Education
Projects
Languages
""");

        Assert.Equal(80, requiredOnly.HeaderQualityScore);
        Assert.Equal(90, withOptional.HeaderQualityScore);
        Assert.Contains("Projects", withOptional.HeadersFound);
        Assert.Contains("Certifications", requiredOnly.HeadersMissing);
    }

    [Fact]
    public void Validate_UnknownLikelyHeaders_AppearInUnclearHeaders()
    {
        var result = CreateValidator().Validate("""
Summary
Skills
Where I Made an Impact
Work Experience
Education
""");

        Assert.Contains("Where I Made an Impact", result.UnclearHeaders);
    }

    [Fact]
    public void Validate_ResumeContentTitles_DoesNotTreatCompaniesAndProjectsAsUnclearHeaders()
    {
        var result = CreateValidator().Validate("""
Yangshun Tay
Skills
Contact
Professional Experience
GreatFrontEnd
Figma to Code Plugin
Meta
Projects
NUSMods
Websites
HTML5 Gaming
Documentation with Docusaurus
of Facebook
Grab
Education
National University of Singapore
""");

        Assert.Empty(result.UnclearHeaders);
        Assert.DoesNotContain("Yangshun Tay", result.UnclearHeaders);
        Assert.DoesNotContain("Contact", result.UnclearHeaders);
        Assert.DoesNotContain("GreatFrontEnd", result.UnclearHeaders);
        Assert.DoesNotContain("Figma to Code Plugin", result.UnclearHeaders);
        Assert.DoesNotContain("Meta", result.UnclearHeaders);
        Assert.DoesNotContain("NUSMods", result.UnclearHeaders);
        Assert.DoesNotContain("Websites", result.UnclearHeaders);
        Assert.DoesNotContain("HTML5 Gaming", result.UnclearHeaders);
        Assert.DoesNotContain("Documentation with Docusaurus", result.UnclearHeaders);
        Assert.DoesNotContain("of Facebook", result.UnclearHeaders);
        Assert.DoesNotContain("Grab", result.UnclearHeaders);
        Assert.DoesNotContain("National University of Singapore", result.UnclearHeaders);
    }

    [Fact]
    public void Validate_ScoreClampsAtOneHundred()
    {
        var result = CreateValidator().Validate("""
Summary
Skills
Work Experience
Education
Projects
Certifications
Awards
Languages
Volunteer Experience
Extra Impact
Career Highlights
Personal Toolkit
""");

        Assert.Equal(100, result.HeaderQualityScore);
    }

    [Fact]
    public void Validate_EmptyResumeText_ReturnsWeakResult()
    {
        var result = CreateValidator().Validate("");

        Assert.Equal(0, result.HeaderQualityScore);
        Assert.Equal("weak", result.StructureQuality);
        Assert.Empty(result.HeadersFound);
        Assert.Equal(9, result.HeadersMissing.Count);
    }

    private static StandardHeaderValidator CreateValidator()
    {
        return new StandardHeaderValidator();
    }
}
