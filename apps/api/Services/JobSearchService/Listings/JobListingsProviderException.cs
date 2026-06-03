namespace ResumeReview.Api.Services.JobSearchService.Listings;

public sealed class JobListingsProviderException : Exception
{
    public JobListingsProviderException(string message)
        : base(message)
    {
    }

    public JobListingsProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
