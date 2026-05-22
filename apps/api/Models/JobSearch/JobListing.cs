namespace ResumeReview.Api.Models ;

public class JobListing
{
    public string ListingTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string WhyItsGreat { get; set; } = string.Empty;
    public string ListingLink { get; set; } = string.Empty;
    public string SearchQuery { get; set; } = string.Empty;
}

public class JobListings
{

    public List<JobListing> jobListings{get; set;} = [] ;
}