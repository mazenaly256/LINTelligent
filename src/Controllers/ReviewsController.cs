using Hangfire;
using LINTelligent.DTOs.Request;
using LINTelligent.DTOs.Response;
using LINTelligent.Entities;
using LINTelligent.Infrastructure.Persistence;
using LINTelligent.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LINTelligent.Controllers;

[ApiController]
[Route("/reviews")]
public class ReviewsController(AppDbContext context, ILLMClient llmClient) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointName("Request a code review.")]
    [EndpointDescription("Lints the code snippet and give a review about it.")]
    public async Task<ActionResult<Review>> ReviewCodeSnippetAsync(CodeReviewRequest codeReviewRequest, CancellationToken ct)
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
            CodeSnippet = codeReviewRequest.CodeSnippet
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
    public async Task<ActionResult<CodeReviewResponse>> GetReviewByIdAsync(Guid reviewId, CancellationToken ct)
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

        CodeReviewResponse reviewDto = new()
        {
            ReviewId = reviewFromDB.Id,
            Language = reviewFromDB.Language,
            CodeSnippet = reviewFromDB.CodeSnippet,
            Status = reviewFromDB.Status,
            Issues = reviewFromDB.Report is null ? null : JsonSerializer.Deserialize<List<CodeIssue>>(reviewFromDB.Report)
        };

        return Ok(reviewDto);
    }
}
