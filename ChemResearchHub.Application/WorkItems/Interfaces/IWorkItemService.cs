using ChemResearchHub.Application.WorkItems.Dtos;
using ChemResearchHub.Domain.Enums;

namespace ChemResearchHub.Application.WorkItems.Interfaces;

public interface IWorkItemService
{
    Task<IReadOnlyList<WorkItemDto>> GetByBoardColumnIdAsync(
        int boardColumnId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkItemDto>> GetAllAsync(
        string? search = null,
        bool? isCompleted = null,
        string? assignedToUserId = null,
        CancellationToken cancellationToken = default);

    Task<WorkItemDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<WorkItemDto?> UpdateAsync(
        int id,
        string title,
        string? description,
        WorkItemType type,
        int priority,
        DateTime? dueDate,
        string? assignedToUserId,
        CancellationToken cancellationToken = default);

    Task<WorkItemDto> CreateAsync(
        int projectId,
        int boardColumnId,
        string title,
        string? description,
        WorkItemType type,
        int priority,
        DateTime? dueDate,
        string? assignedToUserId,
        CancellationToken cancellationToken = default);

    Task<bool> MoveAsync(
        int id,
        int boardColumnId,
        int sortOrder,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ReopenAsync(
        int id,
        CancellationToken cancellationToken = default);
}