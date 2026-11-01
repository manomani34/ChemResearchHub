namespace ChemResearchHub.Application.Notifications.Models;

public sealed class NotificationMessage
{
    public required string UserId { get; init; }
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public string? ActionUrl { get; init; }
}