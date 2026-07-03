using LINTelligent.Application.DTOs;
using LINTelligent.Application.Services.Interfaces;

namespace LINTelligent.Infrastructure.Services;

public class NotificationService(IHttpClientFactory httpClientFactory) : INotificationService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task SendAsync(NotificationMessageDto body, Uri destinationUrl, CancellationToken ct)
    {
        // webhook-based notifying

        var notification = await _httpClient.PostAsJsonAsync(destinationUrl, body, ct);
        notification.EnsureSuccessStatusCode();
    }
}
