using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Application.Notifications.Models;

namespace ChemResearchHub.Infrastructure.Notifications.Services;

public sealed class NotificationDispatcher
{
    private readonly INotificationService _notificationService;

    public NotificationDispatcher(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task<NotificationDispatchResult> DispatchAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        return _notificationService.SendAsync(
            notification,
            cancellationToken);
    }
}