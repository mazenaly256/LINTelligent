using Hangfire;
using LINTelligent.Application.DTOs;
using LINTelligent.Application.Services.Interfaces;
using LINTelligent.Domain;
using LINTelligent.Infrastructure.LLMClients.Implementations.Ollama.DTOs;
using LINTelligent.Infrastructure.LLMClients.Interfaces;
using LINTelligent.Infrastructure.Persistence.Repositories.Implementations;
using LINTelligent.Infrastructure.Persistence.Repositories.Interfaces;
using LINTelligent.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace LINTelligent.Application.Services.Implementations;

public class ReviewService(IReviewRepository reviewRepository, ILLMClient llmClient, INotificationService notificationService) : IReviewService
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
        BackgroundJob.Enqueue<IReviewService>(rs => rs.RequestProcessingAsync(newReviewId, reviewRequest.Language, reviewRequest.CodeSnippet, reviewRequest.WebhookUrl));

        return newReviewId;
    }

    public async Task RequestProcessingAsync(Guid pendingReviewId, string language, string codeSnippet, string? webhookUrl)
    {
        try
        {
            await reviewRepository.ChangeStatusAsync(pendingReviewId, "Processing", CancellationToken.None);

            var llmResponse = await llmClient.GetCodeReviewReportAsync(language, codeSnippet, CancellationToken.None);

            await reviewRepository.AddReportToTheReviewAsync(pendingReviewId, llmResponse.CodeReviewReport, CancellationToken.None);
            
            await reviewRepository.ChangeStatusAsync(pendingReviewId, llmResponse.SuccessfulRequest ? "Completed" : "Failed", CancellationToken.None);

            var reviewFromDB = await reviewRepository.GetReviewByIdAsync(pendingReviewId, CancellationToken.None);

            // Notifying the user
            try
            {
                if (!string.IsNullOrWhiteSpace(webhookUrl))
                {
                    NotificationMessageDto? notificationMessage = NotificationMessageDto.FromModel(reviewFromDB);

                    await notificationService.SendAsync(notificationMessage, new Uri(webhookUrl), CancellationToken.None);
                }
            }

            catch
            {
                // May log here that the notifying process failed
                // swallow the webhook-related issue silently, as it should not cause problems on the flow of job execution
            }
        }

        catch
        {
            // When enters here, then there is a problem and the method will be automatically reexecuted (due to the automatic retry that is configured on the method for Hangfire)
            await reviewRepository.ChangeStatusAsync(pendingReviewId, "Failed", CancellationToken.None);

            throw;      // rethrows the error to trigger Hangfire rescheduling for the job.
        }
        
    }

    public async Task<Review?> GetReviewDetailsAsync(Guid reviewId, CancellationToken ct)
    {
        return await reviewRepository.GetReviewByIdAsync(reviewId, ct);
    }
}
