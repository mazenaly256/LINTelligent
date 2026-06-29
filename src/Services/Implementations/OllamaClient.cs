using LINTelligent.DTOs.Response;
using LINTelligent.Entities;
using LINTelligent.Infrastructure.AI.Ollama;
using LINTelligent.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using System.Net.Http.Headers;

namespace LINTelligent.Services.Implementations;

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

    public async Task<Review> GetCodeReviewReportAsync(string language, string codeSnippet, CancellationToken ct)
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

        var review = new Review()
        {
            Status = ollamaResponse!.Done ? "Completed" : "Failed",
            Report = ollamaResponse.Message.Content
        };

        return review;
    }
}
