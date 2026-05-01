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

    public static IEnumerable<object[]> Schemas()
    {
        yield return ["ats_content", AtsContentSchema.Schema, new[] { "heading", "resumeName", "content" }];
        yield return ["job_match", JobMatchSchema.Schema, new[] { "name", "targetJob", "overallScore" }];
        yield return ["job_recommendation", JobRecommendationSchema.Schema, new[] { "yearsExperience", "jobTitles", "responsibilities" }];
        yield return ["job_search_profile", JobSearchProfileSchema.Schema, new[] { "titles", "keywords", "seniority", "locations", "exclude" }];
        yield return ["spelling_and_grammar", SpellingAndGrammarSchema.Schema, new[] { "score", "grammarSuggestions", "spellingSuggestions" }];
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
