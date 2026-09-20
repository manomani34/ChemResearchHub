using ChemResearchHub.Domain.Enums;

namespace ChemResearchHub.Application.WorkItems.Dtos;

public class WorkItemDto
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public int? BoardColumnId { get; init; }

    public string? AssignedToUserId { get; init; }

    public string? AssignedToUserName { get; init; }

    public string Title { get; init; } = null!;

    public string? Description { get; init; }

    public WorkItemType Type { get; init; }

    public int Priority { get; init; }

    public DateTime? DueDate { get; init; }

    public bool IsCompleted { get; init; }

    public int SortOrder { get; init; }
}