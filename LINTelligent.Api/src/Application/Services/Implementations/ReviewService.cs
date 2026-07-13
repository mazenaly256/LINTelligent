using Hangfire;
using LINTelligent.Application.Contracts.DTOs;
using LINTelligent.Application.Contracts.Interfaces;
using LINTelligent.Application.Services.Interfaces;
using LINTelligent.Domain;

namespace LINTelligent.Application.Services.Implementations;

public class ReviewService(IReviewRepository reviewRepository, IBackgroundJobClient backgroundJobClient, ILLMClient llmClient, IGitHubClient gitHubClient, INotificationService notificationService) : IReviewService
{
    public async Task<Guid> SubmitReviewRequestAsync(NewReviewRequestDto reviewRequest, CancellationToken ct)
    {
        Review newReview = new()
        {
            Language = reviewRequest.Language,
            CodeSnippet = string.IsNullOrWhiteSpace(reviewRequest.CodeSnippet) ? "Fetching from GitHub....." : reviewRequest.CodeSnippet,
            Status = "Pending",
            WebhookUrl = reviewRequest.WebhookUrl,
            Report = null
        };

        var newReviewId = await reviewRepository.AddNewReviewAsync(newReview, ct);

        if (string.IsNullOrWhiteSpace(reviewRequest.GitHubContentUrl))
        {
            // Hangfire always add its own cancellation token, whatever the sent value
            backgroundJobClient.Enqueue<IReviewService>(rs => rs.CallLLMAndPersistReviewReportAsync(newReviewId, CancellationToken.None));
        }

        else
        {
            var fetchedCodeSnippet = backgroundJobClient.Enqueue<IReviewService>(rs => rs.FetchAndPersistTheCodeSnippetFromGitHubAsync(newReviewId, reviewRequest.GitHubContentUrl, CancellationToken.None));

            backgroundJobClient.ContinueJobWith<IReviewService>(fetchedCodeSnippet, rs => rs.CallLLMAndPersistReviewReportAsync(newReviewId, CancellationToken.None));
        }
        

        return newReviewId;
    }


    public async Task FetchAndPersistTheCodeSnippetFromGitHubAsync(Guid reviewId, string gitHubUserContentUrl, CancellationToken ct)
    {
        try
        {
            await reviewRepository.ChangeStatusAsync(reviewId, "Processing", ct);

            string codeSnippet = await gitHubClient.FetchCodeSnippetFromUrlAsync(gitHubUserContentUrl, ct);

            if (codeSnippet.Length > 5000)
            {
                throw new ArgumentException();      // stop the execution of this job and dependent jobs.
            }

            await reviewRepository.PersistCodeSnippetFromGitHub(reviewId, codeSnippet, ct);
        }
        catch
        {
            await reviewRepository.ChangeStatusAsync(reviewId, "Failed", ct);
        }
    }


    public async Task CallLLMAndPersistReviewReportAsync(Guid reviewId, CancellationToken ct)
    {
        try
        {
            await reviewRepository.ChangeStatusAsync(reviewId, "Processing", ct);

            var reviewFromDB = await reviewRepository.GetReviewByIdAsync(reviewId, ct);

            var llmResponse = await llmClient.GetCodeReviewReportAsync(reviewFromDB.Language, reviewFromDB.CodeSnippet, ct);

            await reviewRepository.AddReportToTheReviewAsync(reviewId, llmResponse?.CodeReviewReport!, ct);

            await reviewRepository.ChangeStatusAsync(reviewId, llmResponse is null || !llmResponse.SuccessfulRequest ? "Failed" : "Completed", ct);

            // Notifying the user
            try
            {
                if (!string.IsNullOrWhiteSpace(reviewFromDB.WebhookUrl))
                {
                    NotificationMessageDto? notificationMessage = NotificationMessageDto.FromModel(reviewFromDB);

                    await notificationService.SendAsync(notificationMessage, new Uri(reviewFromDB.WebhookUrl), ct);
                }
            }

            catch
            {
                // May log here that the notifying process failed
                // swallow the webhook-related issue silently, as it should not cause problems on the flow of job execution
                // the exception will not go up as this method will be executed asynchronously by a Hangfire worker
            }
        }

        catch
        {
            // When enters here, then there is a problem and the method will be automatically reexecuted (due to the automatic retry that is configured on the method for Hangfire)
            await reviewRepository.ChangeStatusAsync(reviewId, "Failed", ct);

            throw;      // rethrows the error to trigger Hangfire rescheduling for the job.
        }

    }


    public async Task<Review?> GetReviewDetailsAsync(Guid reviewId, CancellationToken ct)
    {
        return await reviewRepository.GetReviewByIdAsync(reviewId, ct);
    }
}
