using ChemResearchHub.Application.Bale.Interfaces;
using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Application.Notifications.Models;
using ChemResearchHub.Infrastructure.Bale.Configuration;
using Microsoft.Extensions.Options;

namespace ChemResearchHub.Infrastructure.Notifications.Channels;

public sealed class BaleNotificationChannel : INotificationChannel
{
    private readonly IBaleBotClient _baleBotClient;
    private readonly BaleBotOptions _options;

    public BaleNotificationChannel(
        IBaleBotClient baleBotClient,
        IOptions<BaleBotOptions> options)
    {
        _baleBotClient = baleBotClient;
        _options = options.Value;
    }

    public string Name => "Bale";

    public async Task SendAsync(
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        if (_options.GroupChatId == 0)
        {
            throw new InvalidOperationException(
                "Bale group chat ID is not configured.");
        }

        var text =
            $"🔔 **اعلان جدید ChemResearchHub**\n\n" +
            $"📌 {notification.Title}\n\n" +
            $"{notification.Message}";

        if (!string.IsNullOrWhiteSpace(notification.ActionUrl))
        {
            text += $"\n\n🔗 {notification.ActionUrl}";
        }

        var result = await _baleBotClient.SendMessageAsync(
            _options.GroupChatId,
            text,
            cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(
                "Bale did not return a message after sending the group notification.");
        }
    }
}