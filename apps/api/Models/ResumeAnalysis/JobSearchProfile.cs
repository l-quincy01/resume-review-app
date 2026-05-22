namespace ResumeReview.Api.Models;

public class JobSearchProfile
{
    public List<string> Titles { get; set; } = [];
    public List<string> Keywords { get; set; } = [];
    public string Seniority { get; set; } = string.Empty;
    public List<string> Locations { get; set; } = [];
    public List<string> Exclude { get; set; } = [];
}