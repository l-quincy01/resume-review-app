namespace ResumeReview.Api.Services.AtsService.TextExtraction;

public interface IResumeTextExtractor
{
    Task<string> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken = default);
}
