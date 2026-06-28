namespace LINTelligent.DTOs.Request;

public class CodeReviewRequest
{
    public string CodeSnippet { get; set; } = null!;

    public string Language { get; set; } = null!;
}
