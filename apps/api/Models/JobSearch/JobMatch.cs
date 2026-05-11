namespace ResumeReview.Api.Models ;

public class JobMatch
{
    public string? Name { get; set; }
    public List<TargetJobMatch> TargetJob { get; set; } = [];
    public int OverallScore { get; set; }
}