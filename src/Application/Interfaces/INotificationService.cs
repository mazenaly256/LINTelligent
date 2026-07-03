using LINTelligent.Application.DTOs;

namespace LINTelligent.Application.Interfaces;

public interface INotificationService
{
    public Task SendAsync(NotificationBodyDto body, Uri destinationUrl, CancellationToken ct);
}
