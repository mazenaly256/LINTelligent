using Hangfire;
using Hangfire.PostgreSql;
using LINTelligent.Infrastructure.Persistence;
using LINTelligent.Services.Implementations;
using LINTelligent.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

string dbConnectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new KeyNotFoundException("Database connection string is not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString));

builder.Services.AddHttpClient();

builder.Services.AddScoped<ILLMClient, OllamaClient>();

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
