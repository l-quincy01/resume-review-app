
namespace ResumeReview.Api.Models ;
public class AtsSectionContent
{
    public string Section { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    public List<string> Strengths { get; set; } = [];
    public List<string> Weaknesses { get; set; } = [];

    public List<AtsSuggestion> Suggestions { get; set; } = [];

  
    public List<SuggestedRewrite>? SuggestedRewrites { get; set; }

    public int Score { get; set; }
}