namespace LINTelligent.Infrastructure.Clients.Ollama.DTOs;

public class OllamaRequest
{
    public string Model { get; set; }

    public List<OllamaMessage> Messages { get; set; }

    public bool Stream { get; set; }
}
