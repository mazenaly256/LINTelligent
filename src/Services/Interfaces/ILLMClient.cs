using Hangfire;

namespace LINTelligent.Services.Interfaces;

public interface ILLMClient
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 5, 10, 30 })]
    public Task GetCodeReviewReportAsync(Guid pendingReviewId, string language, string codeSnippet, CancellationToken ct);
}
