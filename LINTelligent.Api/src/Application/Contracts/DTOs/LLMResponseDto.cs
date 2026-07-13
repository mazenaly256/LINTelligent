namespace LINTelligent.Application.Contracts.DTOs;

public class LLMResponseDto     // to communicate with llm client in infrastructure layer
{
    public bool SuccessfulRequest { get; set; }
    public string CodeReviewReport { get; set; }
}
