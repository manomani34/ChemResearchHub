using ChemResearchHub.Application.Notifications.Models;

namespace ChemResearchHub.Application.Notifications.Interfaces;

public interface INotificationService
{
    Task<NotificationDispatchResult> SendAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default);
}