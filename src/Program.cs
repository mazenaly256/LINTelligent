using LINTelligent.Data;
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

var app = builder.Build();

app.MapOpenApi();

app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "LINTelligent"));

app.MapControllers();

app.Run();
