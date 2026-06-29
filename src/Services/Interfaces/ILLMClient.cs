using LINTelligent.Entities;

namespace LINTelligent.Services.Interfaces;

public interface ILLMClient
{
    public Task<Review> GetCodeReviewReportAsync(string language, string codeSnippet, CancellationToken ct);
}
