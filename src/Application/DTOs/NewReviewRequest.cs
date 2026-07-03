namespace LINTelligent.Application.DTOs;

public class NewReviewRequest
{
    public string CodeSnippet { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string? WebhookUrl { get; set; }
}
