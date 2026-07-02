using Hangfire;
using LINTelligent.DTOs.Response;
using LINTelligent.Entities;
using LINTelligent.Infrastructure.AI.Ollama;
using LINTelligent.Infrastructure.Persistence;
using LINTelligent.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LINTelligent.Services.Implementations;

public class OllamaClient : ILLMClient
{
    private readonly string _apiKey;
    private readonly string _systemPrompt;
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;


    public OllamaClient(AppDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _context = context;

        _apiKey = configuration["LLM:API_KEY"]
            ?? throw new KeyNotFoundException("LLM API key is not found.");

        _systemPrompt = configuration["LLM:SYSTEM_PROMPT"]
            ?? throw new KeyNotFoundException("System prompt is not found.");

        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(configuration["LLM:HOST"]
            ?? throw new KeyNotFoundException("LLM Host address is not found."));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task GetCodeReviewReportAsync(Guid pendingReviewId, string language, string codeSnippet, CancellationToken ct)
    {
        var review = await _context.Reviews.FindAsync(pendingReviewId, ct);
        review.Status = "Processing";
        await _context.SaveChangesAsync(ct);

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

        try
        {
            var httpResponse = await _httpClient.PostAsJsonAsync("/api/chat", ollamaRequest, ct);
            httpResponse.EnsureSuccessStatusCode();
            var ollamaResponse = await httpResponse.Content.ReadFromJsonAsync<OllamaResponse>(ct);

            review.Status = ollamaResponse!.Done ? "Completed" : "Failed";
            review.Report = ollamaResponse.Message.Content;
            await _context.SaveChangesAsync(ct);

            if (review.Status == "Completed" && !string.IsNullOrWhiteSpace(review.WebhookUrl))
            {
                CodeReviewResponseDto reviewDto = new()
                {
                    ReviewId = review.Id,
                    Language = review.Language,
                    CodeSnippet = review.CodeSnippet,
                    Status = review.Status,
                    Issues = string.IsNullOrWhiteSpace(review.Report) ? null : JsonSerializer.Deserialize<List<CodeIssueDto>>(review.Report)
                };

                try
                {
                    var notification = await _httpClient.PostAsJsonAsync(review.WebhookUrl, review, ct);
                    notification.EnsureSuccessStatusCode();
                }

                catch
                {

                }
            }
        }

        catch
        {
            review.Status = "Failed";
            await _context.SaveChangesAsync(ct);
            throw;      // to apply automatic retry for the action/method.
        }
    }
}
