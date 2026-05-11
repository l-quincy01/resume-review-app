using System.Text.Json;
using ResumeReview.Api.Services.Schemas;
using ResumeReview.Api.Services.Tasks;

namespace ResumeReview.Api.Tests;

public class AiSchemaRegressionTests
{
    [Theory]
    [MemberData(nameof(Schemas))]
    public void Schemas_DisallowAdditionalProperties_AndDeclareRequiredFields(
        string schemaName,
        object schema,
        string[] requiredFields)
    {
        var json = JsonSerializer.Serialize(schema);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.False(string.IsNullOrWhiteSpace(schemaName));
        Assert.Equal("object", root.GetProperty("type").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());

        var required = root
            .GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();

        foreach (var field in requiredFields)
        {
            Assert.Contains(field, required);
            Assert.True(root.GetProperty("properties").TryGetProperty(field, out _));
        }
    }

    [Fact]
    public void Tasks_ExposeStableSchemaNames()
    {
        IAiAnalysisTask<object>[] tasks =
        [
            Cast(new AtsContentTask()),
            Cast(new JobMatchTask()),
            Cast(new JobRecommendationTask()),
            Cast(new JobSearchProfileTask()),
            Cast(new SpellingAndGrammarTask())
        ];

        Assert.Equal(
            ["ats_content", "job_match", "job_recommendation", "job_search_profile", "spelling_and_grammar"],
            tasks.Select(task => task.SchemaName));
    }

    [Fact]
    public void AtsKeywordExtractionSchema_DeclaresRequiredKeywordFields()
    {
        var json = JsonSerializer.Serialize(AtsKeywordExtractionSchema.Schema);
        using var document = JsonDocument.Parse(json);
        var keywordItem = document.RootElement
            .GetProperty("properties")
            .GetProperty("keywords")
            .GetProperty("items");

        Assert.False(keywordItem.GetProperty("additionalProperties").GetBoolean());

        var required = keywordItem
            .GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();

        foreach (var field in new[] { "keyword", "tier", "requirement", "category", "context", "variations" })
        {
            Assert.Contains(field, required);
            Assert.True(keywordItem.GetProperty("properties").TryGetProperty(field, out _));
        }
    }

    [Fact]
    public void AtsContextualKeywordScoringSchema_DeclaresRequiredKeywordScoreFields()
    {
        var json = JsonSerializer.Serialize(AtsContextualKeywordScoringSchema.Schema);
        using var document = JsonDocument.Parse(json);
        var keywordItem = document.RootElement
            .GetProperty("properties")
            .GetProperty("keyword_scores")
            .GetProperty("items");

        Assert.False(keywordItem.GetProperty("additionalProperties").GetBoolean());

        var required = keywordItem
            .GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();

        foreach (var field in new[] { "keyword", "present", "matched_terms", "context_type", "evidence" })
        {
            Assert.Contains(field, required);
            Assert.True(keywordItem.GetProperty("properties").TryGetProperty(field, out _));
        }
    }

    public static IEnumerable<object[]> Schemas()
    {
        yield return ["ats_content", AtsContentSchema.Schema, new[] { "heading", "resumeName", "content" }];
        yield return ["job_match", JobMatchSchema.Schema, new[] { "name", "targetJob", "overallScore" }];
        yield return ["job_recommendation", JobRecommendationSchema.Schema, new[] { "yearsExperience", "jobTitles", "responsibilities" }];
        yield return ["job_search_profile", JobSearchProfileSchema.Schema, new[] { "titles", "keywords", "seniority", "locations", "exclude" }];
        yield return ["spelling_and_grammar", SpellingAndGrammarSchema.Schema, new[] { "score", "grammarSuggestions", "spellingSuggestions" }];
        yield return ["ats_keyword_extraction", AtsKeywordExtractionSchema.Schema, new[] { "job_title", "keywords" }];
        yield return ["ats_contextual_keyword_scoring", AtsContextualKeywordScoringSchema.Schema, new[] { "keyword_scores" }];
    }

    private static IAiAnalysisTask<object> Cast<T>(IAiAnalysisTask<T> task)
    {
        return new TaskAdapter<T>(task);
    }

    private sealed class TaskAdapter<T> : IAiAnalysisTask<object>
    {
        private readonly IAiAnalysisTask<T> _inner;

        public TaskAdapter(IAiAnalysisTask<T> inner)
        {
            _inner = inner;
        }

        public string SchemaName => _inner.SchemaName;
        public object Schema => _inner.Schema;
        public string BuildPrompt(string? jobDescription = null) => _inner.BuildPrompt(jobDescription);
    }
}
