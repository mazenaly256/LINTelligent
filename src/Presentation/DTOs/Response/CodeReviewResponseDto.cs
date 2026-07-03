using LINTelligent.Domain;
using System.Text.Json;

namespace LINTelligent.Presentation.DTOs.Response;

public class CodeReviewResponseDto
{
    public Guid ReviewId { get; set; }

    public string Language { get; set; }

    public string CodeSnippet { get; set; }

    public string  Status { get; set; }

    public List<CodeIssueDto>? Issues { get; set; }

    public static CodeReviewResponseDto? FromModel(Review? review)
    {
        if (review is null)
        {
            return null;
        }

        CodeReviewResponseDto reviewDto = new()
        {
            ReviewId = review.Id,
            Language = review.Language,
            CodeSnippet = review.CodeSnippet,
            Status = review.Status,
            Issues = string.IsNullOrWhiteSpace(review.Report) ? null : JsonSerializer.Deserialize<List<CodeIssueDto>>(review.Report)
        };

        return reviewDto;
    }
}
