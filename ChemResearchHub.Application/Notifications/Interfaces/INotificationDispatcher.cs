using ChemResearchHub.Application.Notifications.Models;

namespace ChemResearchHub.Application.Notifications.Interfaces;

public interface INotificationDispatcher
{
    Task<NotificationDispatchResult> DispatchAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default);

    Task<NotificationDispatchResult> DispatchInAppAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default);

    Task<NotificationDispatchResult> DispatchBaleAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default);
}