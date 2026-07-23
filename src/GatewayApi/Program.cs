using Microsoft.EntityFrameworkCore;
using PearlMetric.GatewayApi.Configuration;
using PearlMetric.GatewayApi.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PearlMetric")
    ?? throw new InvalidOperationException(
        "Connection string 'PearlMetric' is required. Configure it with user secrets or the ConnectionStrings__PearlMetric environment variable.");

builder.Services.AddDbContext<PearlMetricDb>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddOptions<CvWorkerOptions>()
    .Bind(builder.Configuration.GetSection(CvWorkerOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
        "CvWorker:BaseUrl must be an absolute HTTP or HTTPS URL.")
    .Validate(
        options => options.TimeoutSeconds is > 0 and <= 300,
        "CvWorker:TimeoutSeconds must be between 1 and 300 seconds.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ImageStorageOptions>()
    .Bind(builder.Configuration.GetSection(ImageStorageOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.RootPath),
        "ImageStorage:RootPath is required.")
    .ValidateOnStart();

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/health", () => Results.Ok(new
{
    Status = "OK",
    Timestamp = DateTime.UtcNow,
    Pooper = "Stinky"
}));

app.MapGet("/", () => Results.NotFound(new 
{
    status = "Nice One"
}));

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}