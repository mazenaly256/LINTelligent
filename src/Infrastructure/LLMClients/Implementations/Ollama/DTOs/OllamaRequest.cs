namespace LINTelligent.Infrastructure.LLMClients.Implementations.Ollama.DTOs;

public class OllamaRequest
{
    public string Model { get; set; }

    public List<OllamaMessage> Messages { get; set; }

    public bool Stream { get; set; }
}
