namespace LINTelligent.Presentation.DTOs.Request;

public class CodeReviewRequestDto
{
    public string CodeSnippet { get; set; } = null!;

    public string GitHubUserContentFileUrl { get; set; }

    public string Language { get; set; } = null!;

    public string? WebhookUrl { get; set; }
}
