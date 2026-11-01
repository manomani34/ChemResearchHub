using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Application.Notifications.Models;

namespace ChemResearchHub.Application.Notifications.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationChannel> _channels;

    public NotificationService(
        IEnumerable<INotificationChannel> channels)
    {
        _channels = channels;
    }

    public async Task<NotificationDispatchResult> SendAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        var results = new List<NotificationChannelResult>();

        foreach (var channel in _channels)
        {
            try
            {
                await channel.SendAsync(
                    notification,
                    cancellationToken);

                results.Add(new NotificationChannelResult(
                    channel.Name,
                    true));
            }
            catch (Exception ex)
            {
                results.Add(new NotificationChannelResult(
                    channel.Name,
                    false,
                    ex.Message));
            }
        }

        return new NotificationDispatchResult(
            null,
            results);
    }
}