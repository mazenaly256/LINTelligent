using LINTelligent.Application.Contracts.Interfaces;

namespace LINTelligent.Infrastructure.Clients.GitHub;

public class GitHubClient(IHttpClientFactory httpClientFactory) : IGitHubClient
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("GitHubClient");

    public async Task<string> FetchCodeSnippetFromUrlAsync(string gitHubUserContentUrl, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(gitHubUserContentUrl, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }
}
