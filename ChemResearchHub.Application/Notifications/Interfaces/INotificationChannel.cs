using ChemResearchHub.Application.Notifications.Models;

namespace ChemResearchHub.Application.Notifications.Interfaces;

public interface INotificationChannel
{
    string Name { get; }

    Task SendAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default);
}