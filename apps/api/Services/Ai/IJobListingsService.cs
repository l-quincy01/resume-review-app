using ResumeReview.Api.Models ;

public interface IJobListingsService
{
    Task<JobListings> FindJobListingsAsync(
        string aiModel,
        JobSearchProfile profile,
        CancellationToken cancellationToken = default);
}