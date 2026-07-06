namespace LINTelligent.Infrastructure.Clients.Ollama.DTOs;

public class OllamaResponse
{
    public OllamaMessage Message { get; set; }

    public bool Done { get; set; }
}
