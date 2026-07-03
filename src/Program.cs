using Hangfire;
using Hangfire.PostgreSql;
using LINTelligent.Application.Services.Implementations;
using LINTelligent.Application.Services.Interfaces;
using LINTelligent.Infrastructure.LLMClients.Implementations.Ollama;
using LINTelligent.Infrastructure.LLMClients.Interfaces;
using LINTelligent.Infrastructure.Persistence;
using LINTelligent.Infrastructure.Persistence.Repositories.Implementations;
using LINTelligent.Infrastructure.Persistence.Repositories.Interfaces;
using LINTelligent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

string dbConnectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new KeyNotFoundException("Database connection string is not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString));

builder.Services.AddHttpClient();

builder.Services.AddScoped<ILLMClient, OllamaClient>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddOpenApi();

builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(dbConnectionString));
});
builder.Services.AddHangfireServer();

var app = builder.Build();

app.MapOpenApi();

app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "LINTelligent"));

app.MapControllers();

app.UseHangfireDashboard("/hangfire");

app.Run();
