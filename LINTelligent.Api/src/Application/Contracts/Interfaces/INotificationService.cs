using LINTelligent.Application.Contracts.DTOs;

namespace LINTelligent.Application.Contracts.Interfaces;

public interface INotificationService
{
    public Task SendAsync(NotificationMessageDto body, Uri destinationUrl, CancellationToken ct);
}
