using LINTelligent.Application.Contracts.Interfaces;

namespace LINTelligent.Infrastructure.Clients.GitHub;

public class GitHubClient(IHttpClientFactory httpClientFactory) : IGitHubClient
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
}
