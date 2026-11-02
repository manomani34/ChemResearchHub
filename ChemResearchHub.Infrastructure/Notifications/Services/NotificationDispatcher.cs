using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Application.Notifications.Models;

namespace ChemResearchHub.Infrastructure.Notifications.Services;

public sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly IEnumerable<INotificationChannel> _channels;

    public NotificationDispatcher(IEnumerable<INotificationChannel> channels)
    {
        _channels = channels;
    }

    public Task<NotificationDispatchResult> DispatchAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        return DispatchChannelsAsync(
            notification,
            _channels,
            cancellationToken);
    }

    public Task<NotificationDispatchResult> DispatchInAppAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        return DispatchChannelAsync(notification, "InApp", cancellationToken);
    }

    public Task<NotificationDispatchResult> DispatchBaleAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        return DispatchChannelAsync(notification, "Bale", cancellationToken);
    }

    private async Task<NotificationDispatchResult> DispatchChannelAsync(
        NotificationMessage notification,
        string channelName,
        CancellationToken cancellationToken)
    {
        var channel = _channels.FirstOrDefault(
            x => string.Equals(
                x.Name,
                channelName,
                StringComparison.OrdinalIgnoreCase));

        if (channel is null)
        {
            return new NotificationDispatchResult(
                null,
                new[]
                {
                    new NotificationChannelResult(
                        channelName,
                        false,
                        $"Notification channel '{channelName}' is not registered.")
                });
        }

        return await DispatchChannelsAsync(
            notification,
            new[] { channel },
            cancellationToken);
    }

    private static async Task<NotificationDispatchResult> DispatchChannelsAsync(
        NotificationMessage notification,
        IEnumerable<INotificationChannel> channels,
        CancellationToken cancellationToken)
    {
        var results = new List<NotificationChannelResult>();

        foreach (var channel in channels)
        {
            try
            {
                await channel.SendAsync(notification, cancellationToken);

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

        return new NotificationDispatchResult(null, results);
    }
}