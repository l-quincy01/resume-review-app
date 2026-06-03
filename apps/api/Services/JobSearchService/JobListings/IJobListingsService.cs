using ResumeReview.Api.Models;

namespace ResumeReview.Api.Services.JobSearchService.Listings;

public interface IJobListingsService
{
    Task<JobListings> FindJobListingsAsync(
        string apiKey,
        string aiModel,
        JobSearchProfile profile,
        CancellationToken cancellationToken = default);
}
