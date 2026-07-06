namespace LINTelligent.Application.Contracts.DTOs;

public class NewReviewRequestDto       // to communicate with Presentation layer
{
    public string CodeSnippet { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string? WebhookUrl { get; set; }
}
