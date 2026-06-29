using Microsoft.AspNetCore.Mvc;

namespace LINTelligent.DTOs.Response;

public class CodeReviewResponse
{
    public Guid ReviewId { get; set; }

    public string Language { get; set; }

    public string CodeSnippet { get; set; }

    public string  Status { get; set; }

    public List<CodeIssue>? Issues { get; set; }
}
