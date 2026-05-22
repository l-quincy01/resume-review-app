using ResumeReview.Api.Models ;

namespace ResumeReview.Api.Dtos.Responses;

public class ResumeReviewResponse
{
    public JobRecommendation JobRecommendation { get; set; } = new();
    public JobMatch JobMatch { get; set; } = new();
    public AtsContent AtsContent { get; set; } = new();
    public SpellingAndGrammar SpellingAndGrammar { get; set; } = new();
    public JobSearchProfile JobSearchProfile { get; set; } = new();
    public List<string> Warnings { get; set; } = [];
}
