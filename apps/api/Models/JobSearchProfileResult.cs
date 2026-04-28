namespace ResumeReview.Api.Models;

public class JobSearchProfileResult
{
    public JobSearchProfile Profile { get; set; } = new();
    public JobListings JobListings { get; set; } = new();
}