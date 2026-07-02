namespace LINTelligent.DTOs.Request;

public class CodeReviewRequestDto
{
    public string CodeSnippet { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string? WebhookUrl { get; set; }
}
