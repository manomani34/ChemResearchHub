using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.Notification;

public class Notification : BaseEntity
{
    private Notification()
    {
    }

    public Notification(
        string userId,
        string type,
        string title,
        string message,
        string? actionUrl = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type is required.", nameof(type));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required.", nameof(message));

        UserId = userId.Trim();
        Type = type.Trim();
        Title = title.Trim();
        Message = message.Trim();
        ActionUrl = string.IsNullOrWhiteSpace(actionUrl)
            ? null
            : actionUrl.Trim();
    }

    public string UserId { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public string? ActionUrl { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public bool IsRead => ReadAt.HasValue;

    public void MarkAsRead()
    {
        ReadAt ??= DateTime.UtcNow;
    }
}