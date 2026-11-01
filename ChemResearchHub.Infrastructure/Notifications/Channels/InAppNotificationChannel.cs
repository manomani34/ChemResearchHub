using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Application.Notifications.Models;
using ChemResearchHub.Domain.Entities.Notification;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Notifications.Channels;

public sealed class InAppNotificationChannel : INotificationChannel
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationRealtimePublisher _realtimePublisher;

    public InAppNotificationChannel(
        ApplicationDbContext db,
        INotificationRealtimePublisher realtimePublisher)
    {
        _db = db;
        _realtimePublisher = realtimePublisher;
    }

    public string Name => "InApp";

    public async Task SendAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        var entity = new Notification(
            notification.UserId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.ActionUrl);

        _db.Set<Notification>().Add(entity);

        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _realtimePublisher.PublishAsync(
                notification.UserId,
                cancellationToken);
        }
        catch
        {
            // Notification is already persisted.
            // SignalR failure must not invalidate In-App delivery.
        }
    }
}