using Hangfire;
using LINTelligent.Domain;
using LINTelligent.Infrastructure.LLMClients.Interfaces;
using LINTelligent.Infrastructure.Persistence;
using LINTelligent.Presentation.DTOs.Request;
using LINTelligent.Presentation.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace LINTelligent.Presentation.Controllers;

[ApiController]
[Route("/reviews")]
public class ReviewsController(AppDbContext context) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointName("Request a code review.")]
    [EndpointDescription("Lints the code snippet and give a review about it.")]
    public async Task<ActionResult<Review>> ReviewCodeSnippetAsync(CodeReviewRequestDto codeReviewRequest, CancellationToken ct)
    {
        if (codeReviewRequest.CodeSnippet.Length > 500)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Long code snippet.",
                Detail = "Code snippet exceeds the maximum number of allowed characters. Max allowed: 500."
            });
        }

        Review pendingReview = new()
        {
            Language = codeReviewRequest.Language,
            Status = "Pending",
            CodeSnippet = codeReviewRequest.CodeSnippet,
            WebhookUrl = codeReviewRequest.WebhookUrl
        };

        await context.Reviews.AddAsync(pendingReview, ct);
        await context.SaveChangesAsync(ct);

        // Once the endpoint call finishes, the framework marks the cancellation token so any thread that uses it will be a canceled operation and when the token is used again when the worker executes the job, it will directly throw OperationCanceledException
        // So that, never send the cancellation token to the be saved as a parameter for an async job.
        BackgroundJob.Enqueue((ILLMClient llmClient) => llmClient.GetCodeReviewReportAsync(pendingReview.Id, codeReviewRequest.Language, codeReviewRequest.CodeSnippet, CancellationToken.None));

        return Accepted($"/reviews/{pendingReview.Id}");
    }


    [HttpGet("{reviewId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointName("Get code review result by ReviewID")]
    [EndpointDescription("Get the report of the code snippet review")]
    public async Task<ActionResult<CodeReviewResponseDto>> GetReviewByIdAsync(Guid reviewId, CancellationToken ct)
    {
        var reviewFromDB = await context.Reviews.FindAsync(reviewId, ct);

        if (reviewFromDB is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Review is not found.",
                Detail = $"Review with ID: {reviewId} is not found, check the ID and try again."
            });
        }
        var reviewDto = CodeReviewResponseDto.FromModel(reviewFromDB);
       
        return Ok(reviewDto);
    }
}
