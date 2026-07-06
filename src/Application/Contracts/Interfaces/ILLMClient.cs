using LINTelligent.Application.Contracts.DTOs;

namespace LINTelligent.Application.Contracts.Interfaces;

public interface ILLMClient
{
    public Task<LLMResponseDto> GetCodeReviewReportAsync(string language, string codeSnippet, CancellationToken ct);
}
