namespace ResumeReview.Api.Models ;

public class JobRecommendation
{
    public string YearsExperience { get; set; } = string.Empty;
    public List<string> JobTitles { get; set; } = [];
    public List<string> Responsibilities { get; set; } = [];
    public List<string> Seniority { get; set; } = [];
    public List<string> Industry { get; set; } = [];
    public List<string> HardSkills { get; set; } = [];
    public List<string> SoftSkills { get; set; } = [];
    public List<string> CompanySizeFit { get; set; } = [];
    public List<string> CareerTrack { get; set; } = [];
}