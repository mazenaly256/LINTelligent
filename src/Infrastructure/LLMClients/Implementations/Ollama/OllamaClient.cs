using Hangfire;
using LINTelligent.Application.DTOs;
using LINTelligent.Application.Interfaces;
using LINTelligent.Infrastructure.LLMClients.Implementations.Ollama.DTOs;
using LINTelligent.Infrastructure.LLMClients.Interfaces;
using LINTelligent.Infrastructure.Persistence;
using LINTelligent.Infrastructure.Persistence.Repositories.Interfaces;
using LINTelligent.Presentation.DTOs.Response;
using Microsoft.EntityFrameworkCore.Query;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LINTelligent.Infrastructure.LLMClients.Implementations.Ollama;

public class OllamaClient : ILLMClient
{
    private readonly string _apiKey;
    private readonly string _systemPrompt;
    private readonly HttpClient _httpClient;
    private readonly IReviewRepository _reviewRepository;
    private readonly INotificationService _notificationService;


    public OllamaClient(IReviewRepository reviewRepository, INotificationService notificationService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _reviewRepository = reviewRepository;

        _notificationService = notificationService;

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
        await _reviewRepository.ChangeStatusAsync(pendingReviewId, "Processing", ct);

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

            await _reviewRepository.AddReportToTheReviewAsync(pendingReviewId, ollamaResponse!.Done, ollamaResponse.Message.Content, ct);

            var review = await _reviewRepository.GetReviewByIdAsync(pendingReviewId, ct);

            if (!string.IsNullOrWhiteSpace(review.WebhookUrl))
            {
                NotificationBodyDto? notificationBody = NotificationBodyDto.FromModel(review);

                try
                {
                    await _notificationService.SendAsync(notificationBody!, new Uri(review.WebhookUrl), ct);
                }

                catch
                {
                    // May log here that the notifying process failed
                    // This catch is mainly for swallowing the exception to not retry the job due to failing of the notifying.
                }
            }
        }

        catch
        {
            await _reviewRepository.ChangeStatusAsync(pendingReviewId, "Failed", ct);
            throw;      // to apply automatic retry for the action/method.
        }
    }
}
