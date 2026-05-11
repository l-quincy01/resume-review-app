using ResumeReview.Api.Options;
using ResumeReview.Api.Services;
using ResumeReview.Api.Services.Ai.Providers.OpenAi;
using ResumeReview.Api.Services.JobSearchService.Listings;
using ResumeReview.Api.Services.JobSearchService.Providers.OpenAI;
using ResumeReview.Api.Services.Tasks;
using ResumeReview.Api.Services.AtsService.ContextualKeywordScoring;
using ResumeReview.Api.Services.AtsService.FinalAssessment;
using ResumeReview.Api.Services.AtsService.HeaderValidation;
using ResumeReview.Api.Services.AtsService.KeywordExtraction;
using ResumeReview.Api.Services.AtsService.KeywordScoring;
using ResumeReview.Api.Services.AtsService.Providers.OpenAI;
using ResumeReview.Api.Services.AtsService.TextExtraction;
using ResumeReview.Api.Services.ResumeReview;
using ResumeReview.Api.Services.Providers;
using ResumeReview.Api.Services.Providers.OpenAI;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var openAiOptions = builder.Configuration
        .GetSection(OpenAiOptions.SectionName)
        .Get<OpenAiOptions>() ?? new OpenAiOptions();

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Application", "ResumeReview.Api")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
    });

    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.Configure<OpenAiOptions>(
        builder.Configuration.GetSection(OpenAiOptions.SectionName));


    builder.Services.AddScoped<IResumeReviewService, ResumeReviewService>();
    builder.Services.AddScoped<IResumeTextExtractor, PdfPigResumeTextExtractor>();
    builder.Services.AddScoped<IStandardHeaderValidator, StandardHeaderValidator>();
    builder.Services.AddScoped<KeywordExtractionEnricher>();
    builder.Services.AddScoped<KeywordAnalysisMerger>();
    builder.Services.AddScoped<IKeywordScoringService, KeywordScoringService>();
    builder.Services.AddScoped<IFinalAssessmentService, FinalAssessmentService>();


    builder.Services.AddScoped<IAiResumeAnalysisService, ResumeAnalysisService>();


    builder.Services.AddScoped<JobRecommendationTask>();
    builder.Services.AddScoped<JobMatchTask>();
    builder.Services.AddScoped<AtsContentTask>();
    builder.Services.AddScoped<SpellingAndGrammarTask>();
    builder.Services.AddScoped<JobSearchProfileTask>();
    builder.Services.AddSingleton<OpenAiRetryPolicy>();


    builder.Services.AddHttpClient<IAiProviderClient, OpenAiProviderClient>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(openAiOptions.RequestTimeoutSeconds);
    });


    builder.Services.AddHttpClient<IJobListingsService, OpenAiJobListingsService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(openAiOptions.JobListingsTimeoutSeconds);
    });

    builder.Services.AddHttpClient<IKeywordExtractionService, OpenAiKeywordExtractionService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(openAiOptions.RequestTimeoutSeconds);
    });

    builder.Services.AddHttpClient<IKeyWordAnalysisService, OpenAIKeyWordAnalysisService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(openAiOptions.RequestTimeoutSeconds);
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

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
        };
    });

    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "ResumeReview.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
