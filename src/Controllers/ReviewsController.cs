using LINTelligent.DTOs.Request;
using LINTelligent.DTOs.Response;
using LINTelligent.Entities;
using LINTelligent.Infrastructure.Data;
using LINTelligent.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LINTelligent.Controllers;

[ApiController]
[Route("/reviews")]
public class ReviewsController(AppDbContext context, ILLMClient llmClient) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

        var llmReviewResponse = await llmClient.GetCodeReviewReportAsync(codeReviewRequest.Language, codeReviewRequest.CodeSnippet, ct);

        Review fullCodeReview = new()
        {
            Language = codeReviewRequest.Language,
            Status = llmReviewResponse.Status,
            Report = llmReviewResponse.Report,
            CodeSnippet = codeReviewRequest.CodeSnippet
        };

        await context.Reviews.AddAsync(fullCodeReview, ct);
        await context.SaveChangesAsync();

        CodeReviewResponse reviewDto = new()
        {
            ReviewId = fullCodeReview.Id,
            Status = fullCodeReview.Status,
            Language = fullCodeReview.Language,
            CodeSnippet = fullCodeReview.CodeSnippet,
            Issues = JsonSerializer.Deserialize<List<CodeIssue>>(fullCodeReview.Report!)
        };

        return Ok(reviewDto);
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
            Status = reviewFromDB.Status,
            Issues = JsonSerializer.Deserialize<List<CodeIssue>>(reviewFromDB.Report!)
        };

        return Ok(reviewDto);
    }
}
