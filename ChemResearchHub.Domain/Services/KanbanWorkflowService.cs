using ChemResearchHub.Domain.Entities.Board;
using ChemResearchHub.Domain.Entities.WorkItem;

namespace ChemResearchHub.Domain.Services;

public class KanbanWorkflowService
{
    public void MoveWorkItem(
        Board board,
        WorkItem workItem,
        BoardColumn targetColumn,
        int sortOrder)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(workItem);
        ArgumentNullException.ThrowIfNull(targetColumn);

        if (workItem.ProjectId != board.ProjectId)
        {
            throw new InvalidOperationException(
                "The work item does not belong to the board project.");
        }

        var columnBelongsToBoard =
            board.Columns.Any(x => x.Id == targetColumn.Id);

        if (!columnBelongsToBoard)
        {
            throw new InvalidOperationException(
                "The target column does not belong to the board.");
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order cannot be negative.");
        }

        workItem.MoveToColumn(
            targetColumn.Id,
            sortOrder);
    }
}