using LINTelligent.Application.Contracts.Interfaces;

namespace LINTelligent.Infrastructure.Clients.GitHub;

public class GitHubClient(IHttpClientFactory httpClientFactory) : IGitHubClient
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<string> FetchCodeSnippetFromUrlAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
