using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.Providers.OpenAI;
using ResumeReview.Api.Services.Providers;

namespace ResumeReview.Api.Services.Ai.Providers.OpenAi;

public sealed class OpenAiProviderClient : IAiProviderClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly OpenAiRetryPolicy _retryPolicy;
    private readonly ILogger<OpenAiProviderClient> _logger;

    public OpenAiProviderClient(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        OpenAiRetryPolicy retryPolicy,
        ILogger<OpenAiProviderClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _retryPolicy = retryPolicy;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (fileStream.CanSeek)
            fileStream.Position = 0;

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        var fileBytes = memoryStream.ToArray();

        using var response = await _retryPolicy.SendAsync(
            async token =>
            {
                using var multipart = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                multipart.Add(new StringContent("user_data"), "purpose");
                multipart.Add(fileContent, "file", fileName);

                return await _httpClient.PostAsync("files", multipart, token);
            },
            "file upload",
            cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenAI file upload failed. Status: {Status}. ResponseBodyLength: {ResponseBodyLength}.",
                response.StatusCode,
                responseText.Length);

            throw new InvalidOperationException(
                $"OpenAI file upload failed: {response.StatusCode}.");
        }

        using var doc = JsonDocument.Parse(responseText);

        return doc.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("OpenAI file upload did not return an id.");
    }

    public async Task DeleteFileAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _retryPolicy.SendAsync(
            token => _httpClient.DeleteAsync($"files/{Uri.EscapeDataString(fileId)}", token),
            "file delete",
            cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenAI file cleanup failed. FileId: {FileId}. Status: {Status}. ResponseBodyLength: {ResponseBodyLength}.",
                fileId,
                response.StatusCode,
                responseText.Length);

            throw new InvalidOperationException(
                $"OpenAI file cleanup failed for file '{fileId}': {response.StatusCode}.");
        }

        _logger.LogInformation(
            "OpenAI file cleanup succeeded. FileId: {FileId}. Status: {Status}.",
            fileId,
            response.StatusCode);
    }

    public async Task<T> SendStructuredRequestAsync<T>(
        string model,
        string fileId,
        string prompt,
        string schemaName,
        object schema,
        CancellationToken cancellationToken = default)
    {
        var resolvedModel = ResolveModel(model);

        var requestBody = new
        {
            model = resolvedModel,
            input = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "input_text", text = prompt },
                        new { type = "input_file", file_id = fileId }
                    }
                }
            },
            text = new
            {
                verbosity = resolvedModel == _options.DefaultModel ? "medium" : "low",
                format = new
                {
                    type = "json_schema",
                    name = schemaName,
                    strict = true,
                    schema
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        _logger.LogInformation("Sending OpenAI request for schema {SchemaName}", schemaName);

        using var response = await _retryPolicy.SendAsync(
            async token =>
            {
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                return await _httpClient.PostAsync("responses", content, token);
            },
            $"structured request '{schemaName}'",
            cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenAI request failed. Schema: {SchemaName}. Status: {Status}. ResponseBodyLength: {ResponseBodyLength}.",
                schemaName,
                response.StatusCode,
                responseText.Length);

            throw new InvalidOperationException(
                $"OpenAI request failed for schema '{schemaName}': {response.StatusCode}.");
        }

        var modelJson = OpenAiResponseParser.ExtractTextOutput(responseText);

        return JsonSerializer.Deserialize<T>(
            modelJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize '{schemaName}' response.");
    }

    private string ResolveModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return _options.DefaultModel;
        }

        if (_options.AllowedModels.Contains(model, StringComparer.OrdinalIgnoreCase))
        {
            return model;
        }

        _logger.LogWarning(
            "Requested model {Model} is not allowed. Falling back to {DefaultModel}.",
            model,
            _options.DefaultModel);

        return _options.DefaultModel;
    }
}
