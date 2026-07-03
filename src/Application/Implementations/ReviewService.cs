using Hangfire;
using LINTelligent.Application.DTOs;
using LINTelligent.Application.Interfaces;
using LINTelligent.Domain;
using LINTelligent.Infrastructure.LLMClients.Interfaces;
using LINTelligent.Infrastructure.Persistence.Repositories.Interfaces;

namespace LINTelligent.Application.Implementations;

public class ReviewService(IReviewRepository reviewRepository) : IReviewService
{
    public async Task<Guid> SubmitReviewRequestAsync(NewReviewRequest reviewRequest, CancellationToken ct)
    {
        Review newReview = new()
        {
            Language = reviewRequest.Language,
            CodeSnippet = reviewRequest.CodeSnippet,
            Status = "Pending",
            WebhookUrl = reviewRequest.WebhookUrl,
            Report = null
        };

        var newReviewId = await reviewRepository.AddNewReviewAsync(newReview, ct);

        // Once the endpoint call finishes, the framework marks the cancellation token so any thread that uses it will be a canceled operation and when the token is used again when the worker executes the job, it will directly throw OperationCanceledException
        // So that, never send the cancellation token to the be saved as a parameter for an async job.
        BackgroundJob.Enqueue((ILLMClient llmClient) => llmClient.GetCodeReviewReportAsync(newReviewId, newReview.Language, newReview.CodeSnippet, CancellationToken.None));

        return newReviewId;
    }

    public async Task<Review?> GetReviewDetailsAsync(Guid reviewId, CancellationToken ct)
    {
        return await reviewRepository.GetReviewByIdAsync(reviewId, ct);
    }
}
