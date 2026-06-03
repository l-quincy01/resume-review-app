
namespace ResumeReview.Api.Services.Providers;

public interface IAiProviderClient
{
    Task<string> UploadFileAsync(
        string apiKey,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        string apiKey,
        string fileId,
        CancellationToken cancellationToken = default);

    Task<T> SendStructuredRequestAsync<T>(
        string apiKey,
        string model,
        string fileId,
        string prompt,
        string schemaName,
        object schema,
        CancellationToken cancellationToken = default);
}
