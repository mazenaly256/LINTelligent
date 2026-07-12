using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Json;

namespace LINTelligent.McpServer;

[McpServerToolType]
public class ReviewTools(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("LINTelligent Service Api");

    [McpServerTool]
    [Description("Submits a code snippet for review. Returns the new reviewId immediately (review process runs as a backgroud job). If you received a Guid and not null from this method, you MUST call GetReviewDetails with the returned Guid reviewId after 3 to 5 seconds to retrieve the result.")]
    public async Task<Guid?> SubmitReviewRequest(
        [Description("The code snippet to review, as a raw string. Mutually exclusive with gitHubUserContentFileUrl, either give value here or as gitHubUserContentFileUrl. Never both, never neither.")]
        string? codeSnippet,

        [Description("A public raw.githubusercontent.com file URL to review. Mutually exclusive with codeSnippet. either give value here or as gitHubUserContentFileUrl. Never both, never neither.")]
        string? gitHubUserContentFileUrl,

        [Description("The programming language of the code snippet being reviewed. It is Required")]
        string language,

        [Description("Optional URL to receive a webhook notification when the review is finished.")]
        string? webhookUrl,

        CancellationToken ct)
    {
        try
        {
            var requestBody = new
            {
                CodeSnippet = codeSnippet,
                GitHubFileUrl = gitHubUserContentFileUrl,
                Language = language,
                WebhookUrl = webhookUrl
            };

            var response = await _httpClient.PostAsJsonAsync("/reviews", requestBody, ct);
            response.EnsureSuccessStatusCode();

            var newReviewId = Guid.Parse(response.Headers.Location!.Segments[^1]);

            return newReviewId;
        }
        catch
        {
            return null;
        }
        
    }


    [McpServerTool]
    [Description("Retrieves a code review by its reviewId. If status is 'Pending' or 'Processing', then the review is not finished yet and call this tool repeatedly again after a short delay until it gives 'Status' as 'Completed' or 'Failed'. Only recall if it does not return null.")]
    public async Task<string?> GetReviewDetails(
        [Description("The reviewId returned by SubmitReviewRequest, or any reviewId for a previous review request.")]
        Guid reviewId, 

        CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/reviews/{reviewId}", ct);
            response.EnsureSuccessStatusCode();

            var reviewDetails = await response.Content.ReadAsStringAsync(ct);

            return reviewDetails;
        }
        catch
        {
            return null;
        }
    }
}
