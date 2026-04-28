
namespace ResumeReview.Api.Services.Providers;

public interface IAiProviderClient
{
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<T> SendStructuredRequestAsync<T>(
        string model,
        string fileId,
        string prompt,
        string schemaName,
        object schema,
        CancellationToken cancellationToken = default);
}