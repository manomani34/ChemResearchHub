using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.AuditLog;

public class AuditLog : BaseEntity
{
    private AuditLog() { }

    public AuditLog(
        string? userId,
        string action,
        string entityType,
        int? entityId,
        string? description,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action is required.", nameof(action));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));

        Action = action.Trim();
        EntityType = entityType.Trim();
        UserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
        EntityId = entityId;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Metadata = string.IsNullOrWhiteSpace(metadata) ? null : metadata.Trim();
    }

    public string? UserId { get; private set; }
    public string Action { get; private set; } = null!;
    public string EntityType { get; private set; } = null!;
    public int? EntityId { get; private set; }
    public string? Description { get; private set; }
    public string? Metadata { get; private set; }
}
