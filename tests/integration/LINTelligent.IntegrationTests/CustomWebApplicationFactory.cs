using Hangfire;
using LINTelligent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace LINTelligent.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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