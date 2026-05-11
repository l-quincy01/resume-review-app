namespace ResumeReview.Api.Services.AtsEngine;

public interface IResumeTextExtractor
{
    Task<string> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken = default);
}
