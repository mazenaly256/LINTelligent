using LINTelligent.Infrastructure.DTOs;

namespace LINTelligent.Infrastructure.LLMClients.Interfaces;

public interface ILLMClient
{
    public Task<LLMResponse> GetCodeReviewReportAsync(string language, string codeSnippet, CancellationToken ct);
}
