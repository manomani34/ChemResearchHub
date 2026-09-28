using ChemResearchHub.Application.WorkItemTransitions.Dtos;

namespace ChemResearchHub.Application.WorkItemTransitions.Interfaces;

public interface IWorkItemFlowSummaryService
{
    Task<WorkItemFlowSummaryDto> CalculateAsync(
        int boardId,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default);
}
