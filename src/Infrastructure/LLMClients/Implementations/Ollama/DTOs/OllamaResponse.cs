namespace LINTelligent.Infrastructure.LLMClients.Implementations.Ollama.DTOs;

public class OllamaResponse
{
    public OllamaMessage Message { get; set; }

    public bool Done { get; set; }
}
