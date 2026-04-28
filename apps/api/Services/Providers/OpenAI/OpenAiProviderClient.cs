using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ResumeReview.Api.Options;
using ResumeReview.Api.Services.Providers.OpenAI;
using ResumeReview.Api.Services.ResumeReview;
using ResumeReview.Api.Services.Providers;

namespace ResumeReview.Api.Services.Ai.Providers.OpenAi;

public sealed class OpenAiProviderClient : IAiProviderClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiProviderClient> _logger;

    public OpenAiProviderClient(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiProviderClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.Timeout = TimeSpan.FromMinutes(10);
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

        using var multipart = new MultipartFormDataContent();

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        multipart.Add(new StringContent("user_data"), "purpose");
        multipart.Add(fileContent, "file", fileName);

        using var response = await _httpClient.PostAsync("files", multipart, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI file upload failed. Status: {Status}. Body: {Body}",
                response.StatusCode,
                responseText);

            throw new InvalidOperationException(
                $"OpenAI file upload failed: {response.StatusCode} - {responseText}");
        }

        using var doc = JsonDocument.Parse(responseText);

        return doc.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("OpenAI file upload did not return an id.");
    }

    public async Task<T> SendStructuredRequestAsync<T>(
        string model,
        string fileId,
        string prompt,
        string schemaName,
        object schema,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = string.IsNullOrWhiteSpace(model) ? "gpt-4.1-mini" : model,
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
                verbosity = model == "gpt-4.1-mini" ? "medium" : "low",
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
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending OpenAI request for schema {SchemaName}", schemaName);

        using var response = await _httpClient.PostAsync("responses", content, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI request failed. Schema: {SchemaName}. Status: {Status}. Body: {Body}",
                schemaName,
                response.StatusCode,
                responseText);

            throw new InvalidOperationException(
                $"OpenAI request failed for schema '{schemaName}': {response.StatusCode} - {responseText}");
        }

        var modelJson = OpenAiResponseParser.ExtractTextOutput(responseText);

        return JsonSerializer.Deserialize<T>(
            modelJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize '{schemaName}' response.");
    }
}