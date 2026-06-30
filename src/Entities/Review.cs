namespace LINTelligent.Entities;

public class Review
{
    public Guid Id { get; set; }

    public string CodeSnippet { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Report { get; set; }

    public string? WebhookUrl { get; set; }
}
