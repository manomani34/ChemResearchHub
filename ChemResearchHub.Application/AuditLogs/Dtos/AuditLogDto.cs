namespace ChemResearchHub.Application.AuditLogs.Dtos;

public class AuditLogDto
{
    public int Id { get; init; }
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public int? EntityId { get; init; }
    public string? Description { get; init; }
    public string? Metadata { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
