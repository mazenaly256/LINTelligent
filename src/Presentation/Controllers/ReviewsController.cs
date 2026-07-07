using LINTelligent.Application.Contracts.DTOs;
using LINTelligent.Application.Services.Interfaces;
using LINTelligent.Presentation.DTOs.Request;
using LINTelligent.Presentation.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace LINTelligent.Presentation.Controllers;

[ApiController]
[Route("/reviews")]
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointName("Request a code review.")]
    [EndpointDescription("Lints the code snippet and give a review about it.")]
    public async Task<IActionResult> ReviewCodeSnippetAsync(CodeReviewRequestDto codeReviewRequest, CancellationToken ct)
    {
        bool gitHubContentFileUrlExists = !string.IsNullOrWhiteSpace(codeReviewRequest.GitHubUserContentFileUrl);
        bool directCodeSnippetExists = !string.IsNullOrWhiteSpace(codeReviewRequest.CodeSnippet);

        if (gitHubContentFileUrlExists == directCodeSnippetExists)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid data",
                Detail = "It is mandatory to have only one way to request a review, either through GitHub or CodeSnippet field."
            });
        }

        if (gitHubContentFileUrlExists)
        {
            if (Uri.TryCreate(codeReviewRequest.GitHubUserContentFileUrl, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme != Uri.UriSchemeHttps || !string.Equals(uri.Host, "raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Unsupported format for fetching from GitHub",
                        Detail = "Can only fetch code through HTTPS URLs from raw.githubusercontent.com"
                    });
                }
            }

            else
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid GitHub File URL",
                    Detail = "Invalid GitHub Content File URL. Can only fetch code through HTTPS URLs from raw.githubusercontent.com"
                });

            }
        }

        if (directCodeSnippetExists && codeReviewRequest.CodeSnippet!.Length > 5000)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Long code snippet.",
                Detail = "Code snippet exceeds the maximum number of allowed characters (5000 characters)."
            });
        }

        NewReviewRequestDto newReviewRequest = new()
        {
            Language = codeReviewRequest.Language,
            CodeSnippet = codeReviewRequest.CodeSnippet!,
            GitHubContentUrl = codeReviewRequest.GitHubUserContentFileUrl!,
            WebhookUrl = codeReviewRequest.WebhookUrl
        };

        var newReviewId = await reviewService.SubmitReviewRequestAsync(newReviewRequest, ct);

        return Accepted($"/reviews/{newReviewId}");
    }


    [HttpGet("{reviewId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointName("Get code review result by ReviewID")]
    [EndpointDescription("Get the report of the code snippet review")]
    public async Task<ActionResult<CodeReviewResponseDto>> GetReviewByIdAsync(Guid reviewId, CancellationToken ct)
    {
        var reviewFromDB = await reviewService.GetReviewDetailsAsync(reviewId, ct);

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
