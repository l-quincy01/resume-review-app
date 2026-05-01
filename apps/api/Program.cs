using ResumeReview.Api.Options;
using ResumeReview.Api.Services;
using ResumeReview.Api.Services.Ai;
using ResumeReview.Api.Services.Ai.Providers;
using ResumeReview.Api.Services.Ai.Providers.OpenAi;
using ResumeReview.Api.Services.Tasks;
using ResumeReview.Api.Services.ResumeReview;
using ResumeReview.Api.Services.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection(OpenAiOptions.SectionName));


builder.Services.AddScoped<IResumeReviewService, ResumeReviewService>();


builder.Services.AddScoped<IAiResumeAnalysisService, ResumeAnalysisService>();


builder.Services.AddScoped<JobRecommendationTask>();
builder.Services.AddScoped<JobMatchTask>();
builder.Services.AddScoped<AtsContentTask>();
builder.Services.AddScoped<SpellingAndGrammarTask>();
builder.Services.AddScoped<JobSearchProfileTask>();


builder.Services.AddHttpClient<IAiProviderClient, OpenAiProviderClient>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});


builder.Services.AddHttpClient<IJobListingsService, OpenAiJobListingsService>(client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
});

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? [];

if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
{
    allowedOrigins = ["http://localhost:3000"];
}

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "Cors:AllowedOrigins must contain at least one origin outside Development.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
