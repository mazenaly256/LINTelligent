using Microsoft.AspNetCore.Mvc;

namespace LINTelligent.DTOs.Response;

public class CodeReviewResponse
{
    public string  Status { get; set; }
    public List<CodeIssue>? Issues { get; set; }
}
