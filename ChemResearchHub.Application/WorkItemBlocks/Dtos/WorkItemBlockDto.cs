namespace ChemResearchHub.Application.WorkItemBlocks.Dtos;

public class WorkItemBlockDto
{
    public int Id { get; init; }
    public int WorkItemId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? BlockedByUserId { get; init; }
    public string? BlockedByUserName { get; init; }
    public DateTime BlockedAtUtc { get; init; }
    public string? UnblockedByUserId { get; init; }
    public string? UnblockedByUserName { get; init; }
    public DateTime? UnblockedAtUtc { get; init; }
    public string? UnblockNote { get; init; }
    public bool IsActive => !UnblockedAtUtc.HasValue;
}
