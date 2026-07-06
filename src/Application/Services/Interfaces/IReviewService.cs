using Hangfire;
using LINTelligent.Application.Contracts.DTOs;
using LINTelligent.Domain;

namespace LINTelligent.Application.Services.Interfaces;

public interface IReviewService
{
    public Task<Guid> SubmitReviewRequestAsync(NewReviewRequestDto reviewRequest, CancellationToken ct);

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 5, 10, 30 })]
    public Task CallLLMAndPersistReviewReportAsync(Guid reviewId, CancellationToken ct);

    public Task FetchAndPersistTheCodeSnippetFromGitHubAsync(Guid reviewId, string gitHubUserContentUrl, CancellationToken ct);

    public Task<Review?> GetReviewDetailsAsync(Guid reviewId, CancellationToken ct);
}
