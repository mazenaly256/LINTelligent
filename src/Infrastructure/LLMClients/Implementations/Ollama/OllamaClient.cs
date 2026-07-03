using LINTelligent.Infrastructure.DTOs;
using LINTelligent.Infrastructure.LLMClients.Implementations.Ollama.DTOs;
using LINTelligent.Infrastructure.LLMClients.Interfaces;
using System.Net.Http.Headers;

namespace LINTelligent.Infrastructure.LLMClients.Implementations.Ollama;

public class OllamaClient : ILLMClient
{
    private readonly string _apiKey;
    private readonly string _systemPrompt;
    private readonly HttpClient _httpClient;


    public OllamaClient(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _apiKey = configuration["LLM:API_KEY"]
            ?? throw new KeyNotFoundException("LLM API key is not found.");

        _systemPrompt = configuration["LLM:SYSTEM_PROMPT"]
            ?? throw new KeyNotFoundException("System prompt is not found.");

        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(configuration["LLM:HOST"]
            ?? throw new KeyNotFoundException("LLM Host address is not found."));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<LLMResponse> GetCodeReviewReportAsync(Guid pendingReviewId, string language, string codeSnippet, CancellationToken ct)
    {
        var ollamaRequest = new OllamaRequest       // One prompt, two sections identifying the Persona/Role and Task
        {
            Model = "gpt-oss:120b-cloud",
            Messages = new List<OllamaMessage>
            {
                new OllamaMessage { Role = "system", Content = _systemPrompt },
                new OllamaMessage { Role = "user", Content = $"Language: {language}\n\nCode:\n{codeSnippet}" }
            },
            Stream = false
        };

        var httpResponse = await _httpClient.PostAsJsonAsync("/api/chat", ollamaRequest, ct);
        httpResponse.EnsureSuccessStatusCode();
        var ollamaResponse = await httpResponse.Content.ReadFromJsonAsync<OllamaResponse>(ct);

        LLMResponse llmReponse = new()
        {
            CodeReviewReport = ollamaResponse.Message.Content,
            SuccessfulRequest = ollamaResponse.Done
        };

        return llmReponse;
    }
}
