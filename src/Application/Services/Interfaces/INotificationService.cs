using LINTelligent.Application.DTOs;

namespace LINTelligent.Application.Services.Interfaces;

public interface INotificationService
{
    public Task SendAsync(NotificationMessageDto body, Uri destinationUrl, CancellationToken ct);
}
