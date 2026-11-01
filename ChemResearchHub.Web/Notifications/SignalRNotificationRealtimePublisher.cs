using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ChemResearchHub.Web.Notifications;

public sealed class SignalRNotificationRealtimePublisher
    : INotificationRealtimePublisher
{
    private readonly IHubContext<WorkItemAlertHub> _hubContext;

    public SignalRNotificationRealtimePublisher(
        IHubContext<WorkItemAlertHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(WorkItemAlertHub.GetUserGroup(userId))
            .SendAsync("NotificationReceived", cancellationToken);
    }
}