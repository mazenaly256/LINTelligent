namespace LINTelligent.Application.Contracts.Interfaces;

public interface IGitHubClient
{
    public Task<string> FetchCodeSnippetFromUrlAsync(string gitHubUserContentUrl, CancellationToken ct);
}
