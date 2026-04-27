var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

var app = builder.Build();

// Configure middleware
app.UseAuthorization();

// Map endpoints
app.MapControllers();

app.Run();