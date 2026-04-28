namespace ResumeReview.Api.Models ;

public class TargetJobMatch
{
    public int Score { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}