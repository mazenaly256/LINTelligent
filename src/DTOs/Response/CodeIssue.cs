namespace LINTelligent.DTOs.Response;

public class CodeIssue
{
    public int LineNumber { get; set; }
    public string Severity { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
}
