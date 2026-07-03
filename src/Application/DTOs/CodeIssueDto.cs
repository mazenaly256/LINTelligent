using System.Text.Json.Serialization;

namespace LINTelligent.Application.DTOs;

public class CodeIssueDto
{
    [JsonPropertyName("lineNumber")]
    public int? LineNumber { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = null!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;
}
