using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.WorkItems;

public class WorkItemsIndexViewModel
{
    public string? Search { get; init; }

    public string Status { get; init; }
        = "all";

    public string? AssignedToUserId { get; init; }

    public IReadOnlyList<WorkItemListItemViewModel> Items { get; init; }
        = Array.Empty<WorkItemListItemViewModel>();

    public IReadOnlyList<WorkItemAssigneeViewModel> Assignees { get; init; }
        = Array.Empty<WorkItemAssigneeViewModel>();
}


public class WorkItemListItemViewModel
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public int? BoardColumnId { get; init; }

    public string Title { get; init; }
        = string.Empty;

    public string? Description { get; init; }

    public string Type { get; init; }
        = string.Empty;

    public int Priority { get; init; }

    public DateTime? DueDate { get; init; }

    public bool IsCompleted { get; init; }

    public string? AssignedToUserName { get; init; }
}


public class WorkItemAssigneeViewModel
{
    public string Id { get; init; }
        = string.Empty;

    public string FullName { get; init; }
        = string.Empty;
}
