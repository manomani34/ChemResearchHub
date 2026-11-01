using ChemResearchHub.Application.Bale.Interfaces;
using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Application.Notifications.Models;
using ChemResearchHub.Infrastructure.Identity;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Notifications.Channels;

public sealed class BaleNotificationChannel : INotificationChannel
{
    private readonly ApplicationDbContext _db;
    private readonly IBaleBotClient _baleBotClient;

    public BaleNotificationChannel(
        ApplicationDbContext db,
        IBaleBotClient baleBotClient)
    {
        _db = db;
        _baleBotClient = baleBotClient;
    }

    public string Name => "Bale";

    public async Task SendAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Where(x => x.Id == notification.UserId)
            .Select(x => new
            {
                x.BaleChatId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
            throw new InvalidOperationException("User was not found.");

        if (!user.BaleChatId.HasValue)
            throw new InvalidOperationException(
                "The user has not linked a Bale account.");

        var text =
            $"🔔 {notification.Title}\n\n" +
            $"{notification.Message}";

        if (!string.IsNullOrWhiteSpace(notification.ActionUrl))
            text += $"\n\n{notification.ActionUrl}";

        var result = await _baleBotClient.SendMessageAsync(
            user.BaleChatId.Value,
            text,
            cancellationToken);

        if (result is null)
            throw new InvalidOperationException(
                "Bale did not return a message.");
    }
}