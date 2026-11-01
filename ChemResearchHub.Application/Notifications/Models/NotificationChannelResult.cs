namespace ChemResearchHub.Application.Notifications.Models;

public sealed record NotificationChannelResult(
    string Channel,
    bool Succeeded,
    string? Error = null);