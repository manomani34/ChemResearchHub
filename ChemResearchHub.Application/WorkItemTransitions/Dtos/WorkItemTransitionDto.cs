namespace ChemResearchHub.Application.WorkItemTransitions.Dtos;

public class WorkItemTransitionDto
{
    public int Id { get; init; }
    public int WorkItemId { get; init; }
    public int BoardId { get; init; }
    public string WorkItemTitle { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    public int? FromBoardColumnId { get; init; }
    public string? FromBoardColumnName { get; init; }
    public int ToBoardColumnId { get; init; }
    public string ToBoardColumnName { get; init; } = string.Empty;
    public string? ChangedByUserId { get; init; }
    public string? ChangedByUserName { get; init; }
    public DateTime ChangedAtUtc { get; init; }
}
