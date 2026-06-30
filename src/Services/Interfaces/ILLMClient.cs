using LINTelligent.Entities;

namespace LINTelligent.Services.Interfaces;

public interface ILLMClient
{
    public Task GetCodeReviewReportAsync(Guid pendingReviewId, string language, string codeSnippet, CancellationToken ct);
}
