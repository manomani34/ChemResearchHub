using ChemResearchHub.Application.Boards.Dtos;
using ChemResearchHub.Application.WorkItems.Dtos;

namespace ChemResearchHub.Web.Models.Boards;

public class KanbanViewModel
{
    public BoardDto Board { get; init; } = null!;

    public IReadOnlyDictionary<int, IReadOnlyList<WorkItemDto>> WorkItemsByColumn
    { get; init; }
        = new Dictionary<int, IReadOnlyList<WorkItemDto>>();
}