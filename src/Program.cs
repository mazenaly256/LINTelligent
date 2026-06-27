using LINTelligent.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

string dbConnectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new KeyNotFoundException("Database connection string is not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString));

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.MapControllers();

app.Run();
