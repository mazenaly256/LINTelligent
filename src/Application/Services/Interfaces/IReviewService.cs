using Hangfire;
using LINTelligent.Application.DTOs;
using LINTelligent.Domain;

namespace LINTelligent.Application.Services.Interfaces;

public interface IReviewService
{
    public Task<Guid> SubmitReviewRequestAsync(NewReviewRequest reviewRequest, CancellationToken ct);

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 5, 10, 30 })]
    public Task RequestProcessingAsync(Guid pendingReviewId, string language, string codeSnippetm, string? webhookUrl);

    public Task<Review?> GetReviewDetailsAsync(Guid reviewId, CancellationToken ct);
}
