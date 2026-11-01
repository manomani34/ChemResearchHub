namespace ChemResearchHub.Application.Notifications.Interfaces;

public interface INotificationRealtimePublisher
{
    Task PublishAsync(
        string userId,
        CancellationToken cancellationToken = default);
}