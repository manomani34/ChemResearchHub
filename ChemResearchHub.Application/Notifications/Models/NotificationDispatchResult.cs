namespace ChemResearchHub.Application.Notifications.Models;

public sealed record NotificationDispatchResult(
    int? NotificationId,
    IReadOnlyList<NotificationChannelResult> Channels)
{
    public bool InAppSucceeded =>
        Channels.Any(x =>
            x.Channel == "InApp" && x.Succeeded);

    public bool BaleSucceeded =>
        Channels.Any(x =>
            x.Channel == "Bale" && x.Succeeded);
}