using Hangfire;
using LINTelligent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace LINTelligent.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            // this runs before Program.cs reads configuration. Overrides data that exists in appsettings.json
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=dummy_host;Database=dummy_database;Username=dummy_username;Password=dummy_password",
                ["LLM:API_KEY"] = "dummy_api_key_for_testing",
                ["LLM:SYSTEM_PROMPT"] = "dummy_api_key_for_testing",
                ["LLM:HOST"] = "http://dummy_llm_host_for_testing"
            });
        });


        builder.ConfigureTestServices(services =>
        {
            // Remove real PostgreSQL DbContextOptions
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Add EF InMemory
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb", DatabaseRoot)
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider()));

            // Remove real Hangfire PostgreSQL storage
            var hangfireDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(JobStorage));

            if (hangfireDescriptor != null)
                services.Remove(hangfireDescriptor);

            services.AddHangfire(config => config.UseInMemoryStorage());
        });
    }
}