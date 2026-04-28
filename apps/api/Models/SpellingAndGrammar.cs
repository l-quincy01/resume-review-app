namespace ResumeReview.Api.Models;

public class WritingSuggestion
{
    public string Type { get; set; } = string.Empty;
    public string Current { get; set; } = string.Empty;
    public string SuggestedCorrection { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public class SpellingAndGrammar
{
    public int Score { get; set; }

    public List<WritingSuggestion> GrammarSuggestions { get; set; } = [];
    public List<WritingSuggestion> SpellingSuggestions { get; set; } = [];
    public List<WritingSuggestion> PersonalPronounCheck { get; set; } = [];
    public List<WritingSuggestion> PassiveVoiceCheck { get; set; } = [];
}