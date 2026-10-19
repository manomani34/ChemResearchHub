using ChemResearchHub.Application.AuditLogs.Interfaces;
using ChemResearchHub.Application.Boards.Repositories;
using ChemResearchHub.Application.Users.Dtos;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.WorkItemAssignments.Repositories;
using ChemResearchHub.Application.WorkItems.Dtos;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Application.WorkItems.Repositories;
using ChemResearchHub.Application.WorkItemTransitions.Repositories;
using ChemResearchHub.Domain.Entities.WorkItem;
using ChemResearchHub.Domain.Enums;

namespace ChemResearchHub.Application.WorkItems.Services;

public class WorkItemService : IWorkItemService
{
    private readonly IWorkItemRepository _workItemRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly IUserService _userService;
    private readonly IWorkItemTransitionRepository _workItemTransitionRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IWorkItemAssignmentRepository _workItemAssignmentRepository;

    public WorkItemService(
        IWorkItemRepository workItemRepository,
        IBoardRepository boardRepository,
        IUserService userService,
        IWorkItemTransitionRepository workItemTransitionRepository,
        IAuditLogService auditLogService,
        IWorkItemAssignmentRepository workItemAssignmentRepository)
    {
        _workItemRepository = workItemRepository;
        _boardRepository = boardRepository;
        _userService = userService;
        _workItemTransitionRepository = workItemTransitionRepository;
        _auditLogService = auditLogService;
        _workItemAssignmentRepository = workItemAssignmentRepository;
    }

    
    public async Task<IReadOnlyList<WorkItemDto>> GetByBoardColumnIdAsync(
        int boardColumnId,
        CancellationToken cancellationToken = default)
    {
        var workItems = await _workItemRepository.GetByBoardColumnIdAsync(
            boardColumnId,
            cancellationToken);

        var users = await _userService.GetAllAsync(cancellationToken);
        var usersById = users.ToDictionary(x => x.Id, x => x);

        var assignments = await _workItemAssignmentRepository
            .GetUserIdsByWorkItemIdsAsync(
                workItems.Select(x => x.Id).ToArray(),
                cancellationToken);

        return workItems
            .Select(x => MapToDto(x, usersById, assignments))
            .ToList();
    }

    public async Task<IReadOnlyList<WorkItemDto>> GetAllAsync(
        string? search = null,
        bool? isCompleted = null,
        string? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        var workItems = await _workItemRepository.GetAllAsync(
            search,
            isCompleted,
            assignedToUserId,
            cancellationToken);

        var users = await _userService.GetAllAsync(cancellationToken);
        var usersById = users.ToDictionary(x => x.Id, x => x);

        var assignments = await _workItemAssignmentRepository
            .GetUserIdsByWorkItemIdsAsync(
                workItems.Select(x => x.Id).ToArray(),
                cancellationToken);

        return workItems
            .Select(x => MapToDto(x, usersById, assignments))
            .ToList();
    }

    public async Task<WorkItemDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var workItem = await _workItemRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (workItem is null)
        {
            return null;
        }

        var users = await _userService.GetAllAsync(cancellationToken);
        var usersById = users.ToDictionary(x => x.Id, x => x);

        var assignments = await _workItemAssignmentRepository
            .GetUserIdsByWorkItemIdAsync(id, cancellationToken);

        return MapToDto(
            workItem,
            usersById,
            new Dictionary<int, IReadOnlyList<string>>
            {
                [id] = assignments
            });
    }

    public async Task<WorkItemDto?> UpdateAsync(
        int id,
        string title,
        string? description,
        WorkItemType type,
        int priority,
        DateTime? dueDate,
        string? assignedToUserId,
        string? changedByUserId,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? assignedUserIds = null)
    {
        var workItem = await _workItemRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (workItem is null)
        {
            return null;
        }

        await ValidateAssignedUserAsync(
            assignedToUserId,
            cancellationToken);

        // If callers omit the collaborator list, preserve existing assignments.
        // An explicitly supplied empty list clears all additional assignments.
        var collaboratorIds = assignedUserIds is null
            ? await _workItemAssignmentRepository.GetUserIdsByWorkItemIdAsync(
                workItem.Id,
                cancellationToken)
            : await NormalizeAndValidateCollaboratorsAsync(
                assignedUserIds,
                assignedToUserId,
                cancellationToken);

        if (assignedUserIds is null)
        {
            collaboratorIds = await NormalizeAndValidateCollaboratorsAsync(
                collaboratorIds,
                assignedToUserId,
                cancellationToken);
        }

        workItem.Update(
            title,
            description,
            type,
            priority,
            dueDate);

        if (string.IsNullOrWhiteSpace(assignedToUserId))
        {
            workItem.Unassign();
        }
        else
        {
            workItem.AssignTo(assignedToUserId.Trim());
        }

        await _workItemAssignmentRepository.ReplaceAssignmentsAsync(
            workItem.Id,
            collaboratorIds,
            changedByUserId,
            cancellationToken);

        // Both repositories use the same scoped DbContext.
        await _workItemRepository.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            changedByUserId,
            "Updated",
            "WorkItem",
            workItem.Id,
            $"Work Item '{workItem.Title}' was updated.",
            cancellationToken: cancellationToken);

        var users = await _userService.GetAllAsync(cancellationToken);
        var usersById = users.ToDictionary(x => x.Id, x => x);

        var savedCollaboratorIds =
            await _workItemAssignmentRepository.GetUserIdsByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        return MapToDto(
            workItem,
            usersById,
            new Dictionary<int, IReadOnlyList<string>>
            {
                [workItem.Id] = savedCollaboratorIds
            });
    }

    public async Task<WorkItemDto> CreateAsync(
        int projectId,
        int boardColumnId,
        string title,
        string? description,
        WorkItemType type,
        int priority,
        DateTime? dueDate,
        string? assignedToUserId,
        string? changedByUserId,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? assignedUserIds = null)
    {
        var board = await _boardRepository.GetByColumnIdAsync(
            boardColumnId,
            cancellationToken);

        if (board is null)
        {
            throw new InvalidOperationException(
                "The specified board column does not exist.");
        }

        if (board.ProjectId != projectId)
        {
            throw new InvalidOperationException(
                "The board column does not belong to the specified project.");
        }

        await ValidateAssignedUserAsync(
            assignedToUserId,
            cancellationToken);

        // Validate additional collaborators before creating the work item.
        var collaboratorIds = await NormalizeAndValidateCollaboratorsAsync(
            assignedUserIds,
            assignedToUserId,
            cancellationToken);

        var existingItems = await _workItemRepository.GetByBoardColumnIdAsync(
            boardColumnId,
            cancellationToken);

        var targetColumn = board.Columns.FirstOrDefault(
            x => x.Id == boardColumnId);

        if (targetColumn is null)
        {
            throw new InvalidOperationException(
                "The specified board column does not exist.");
        }

        if (targetColumn.WipLimit.HasValue &&
            existingItems.Count >= targetColumn.WipLimit.Value)
        {
            throw new InvalidOperationException(
                $"WIP limit reached for column '{targetColumn.Name}'.");
        }

        var nextSortOrder = existingItems.Count;

        var workItem = new WorkItem(
            projectId,
            title,
            type);

        workItem.Update(
            title,
            description,
            type,
            priority,
            dueDate);

        workItem.MoveToColumn(
            boardColumnId,
            nextSortOrder);

        if (!string.IsNullOrWhiteSpace(assignedToUserId))
        {
            workItem.AssignTo(assignedToUserId.Trim());
        }

        await _workItemRepository.AddAsync(
            workItem,
            cancellationToken);

        await _workItemTransitionRepository.AddAsync(
            new ChemResearchHub.Domain.Entities.WorkItemTransition.WorkItemTransition(
                workItem,
                null,
                boardColumnId,
                changedByUserId),
            cancellationToken);

        // Save first so the new work item has its generated ID.
        await _workItemRepository.SaveChangesAsync(cancellationToken);

        if (collaboratorIds.Count > 0)
        {
            await _workItemAssignmentRepository.ReplaceAssignmentsAsync(
                workItem.Id,
                collaboratorIds,
                changedByUserId,
                cancellationToken);

            await _workItemRepository.SaveChangesAsync(cancellationToken);
        }

        await _auditLogService.AddAsync(
            changedByUserId,
            "Created",
            "WorkItem",
            workItem.Id,
            $"Work Item '{workItem.Title}' was created.",
            cancellationToken: cancellationToken);

        var users = await _userService.GetAllAsync(cancellationToken);
        var usersById = users.ToDictionary(x => x.Id, x => x);

        var savedCollaboratorIds =
            await _workItemAssignmentRepository.GetUserIdsByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        return MapToDto(
            workItem,
            usersById,
            new Dictionary<int, IReadOnlyList<string>>
            {
                [workItem.Id] = savedCollaboratorIds
            });
    }

    public async Task<bool> MoveAsync(
        int id,
        int boardColumnId,
        int sortOrder,
        string? changedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order cannot be negative.");
        }

        var workItem =
            await _workItemRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (workItem is null)
        {
            return false;
        }

        var targetBoard =
            await _boardRepository.GetByColumnIdAsync(
                boardColumnId,
                cancellationToken);

        if (targetBoard is null)
        {
            throw new InvalidOperationException(
                "The specified board column does not exist.");
        }

        if (targetBoard.ProjectId != workItem.ProjectId)
        {
            throw new InvalidOperationException(
                "The target board column does not belong to the work item's project.");
        }

        var targetItems =
            await _workItemRepository.GetByBoardColumnIdAsync(
                boardColumnId,
                cancellationToken);

        targetItems =
            targetItems
                .Where(x => x.Id != workItem.Id)
                .ToList();

        var targetColumn =
            targetBoard.Columns.FirstOrDefault(
                x => x.Id == boardColumnId);

        if (targetColumn is null)
        {
            throw new InvalidOperationException(
                "The specified board column does not exist.");
        }

        var currentColumnId =
            workItem.BoardColumnId;

        // The Done workflow column is the source of truth for completion.
        // Moving to Done marks the Work Item completed;
        // moving out of Done reopens it.
        var isDoneColumn =
            string.Equals(
                targetColumn.Name?.Trim(),
                "Done",
                StringComparison.OrdinalIgnoreCase);

        if (isDoneColumn)
        {
            workItem.Complete();
        }
        else
        {
            workItem.Reopen();
        }

        var isMovingToAnotherColumn =
            !currentColumnId.HasValue ||
            currentColumnId.Value != boardColumnId;

        if (isMovingToAnotherColumn &&
            targetColumn.WipLimit.HasValue &&
            targetItems.Count >= targetColumn.WipLimit.Value)
        {
            throw new InvalidOperationException(
                $"WIP limit reached for column '{targetColumn.Name}'.");
        }

        IReadOnlyList<WorkItem> sourceItems =
            Array.Empty<WorkItem>();

        if (currentColumnId.HasValue &&
            currentColumnId.Value != boardColumnId)
        {
            sourceItems =
                await _workItemRepository.GetByBoardColumnIdAsync(
                    currentColumnId.Value,
                    cancellationToken);

            sourceItems =
                sourceItems
                    .Where(x => x.Id != workItem.Id)
                    .ToList();
        }

        var targetIndex =
            Math.Min(
                sortOrder,
                targetItems.Count);

        var reorderedTargetItems =
            targetItems.ToList();

        reorderedTargetItems.Insert(
            targetIndex,
            workItem);

        for (var index = 0;
             index < reorderedTargetItems.Count;
             index++)
        {
            reorderedTargetItems[index]
                .MoveToColumn(
                    boardColumnId,
                    index);
        }

        for (var index = 0;
             index < sourceItems.Count;
             index++)
        {
            sourceItems[index]
                .MoveToColumn(
                    currentColumnId!.Value,
                    index);
        }

        if (isMovingToAnotherColumn)
        {
            await _workItemTransitionRepository.AddAsync(
                new ChemResearchHub.Domain.Entities.WorkItemTransition.WorkItemTransition(
                    workItem,
                    currentColumnId,
                    boardColumnId,
                    changedByUserId),
                cancellationToken);
        }

        await _workItemRepository.SaveChangesAsync(
            cancellationToken);

        if (isMovingToAnotherColumn)
        {
            await _auditLogService.AddAsync(
                changedByUserId,
                "Moved",
                "WorkItem",
                workItem.Id,
                $"Work Item '{workItem.Title}' moved from column {currentColumnId?.ToString() ?? "None"} to column {boardColumnId}.",
                cancellationToken: cancellationToken);
        }

        return true;
    }

    public async Task<bool> CompleteAsync(
    int id,
    string? changedByUserId,
    CancellationToken cancellationToken = default)
    {
        var workItem =
            await _workItemRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (workItem is null)
        {
            return false;
        }

        workItem.Complete();

        await _workItemRepository.SaveChangesAsync(
            cancellationToken);

        await _auditLogService.AddAsync(
            changedByUserId,
            "Completed",
            "WorkItem",
            workItem.Id,
            $"Work Item '{workItem.Title}' was completed.",
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> ReopenAsync(
        int id,
        string? changedByUserId,
        CancellationToken cancellationToken = default)
    {
        var workItem =
            await _workItemRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (workItem is null)
        {
            return false;
        }

        workItem.Reopen();

        await _workItemRepository.SaveChangesAsync(
            cancellationToken);

        await _auditLogService.AddAsync(
            changedByUserId,
            "Reopened",
            "WorkItem",
            workItem.Id,
            $"Work Item '{workItem.Title}' was reopened.",
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
    int id,
    string? changedByUserId,
    CancellationToken cancellationToken = default)
    {
        var workItem =
            await _workItemRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (workItem is null)
        {
            return false;
        }

        var workItemTitle =
            workItem.Title;

        _workItemRepository.Delete(
            workItem);

        await _workItemRepository.SaveChangesAsync(
            cancellationToken);

        await _auditLogService.AddAsync(
            changedByUserId,
            "Deleted",
            "WorkItem",
            id,
            $"Work Item '{workItemTitle}' was deleted.",
            cancellationToken: cancellationToken);

        return true;
    }

    private async Task ValidateAssignedUserAsync(
        string? assignedToUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(assignedToUserId))
        {
            return;
        }

        var user =
            await _userService.GetByIdAsync(
                assignedToUserId,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "The specified user does not exist.");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException(
                "The specified user is inactive.");
        }
    }

    private async Task<IReadOnlyList<string>> NormalizeAndValidateCollaboratorsAsync(
        IReadOnlyCollection<string>? assignedUserIds,
        string? primaryUserId,
        CancellationToken cancellationToken)
    {
        var normalizedIds = (assignedUserIds ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Where(x => string.IsNullOrWhiteSpace(primaryUserId) ||
                        !string.Equals(x, primaryUserId.Trim(), StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        foreach (var userId in normalizedIds)
        {
            await ValidateAssignedUserAsync(userId, cancellationToken);
        }

        return normalizedIds;
    }

    private static WorkItemDto MapToDto(
        WorkItem workItem,
        IReadOnlyDictionary<string, UserDto> usersById,
        IReadOnlyDictionary<int, IReadOnlyList<string>> assignmentsByWorkItemId)
    {
        usersById.TryGetValue(
            workItem.AssignedToUserId ?? string.Empty,
            out var primaryUser);

        assignmentsByWorkItemId.TryGetValue(
            workItem.Id,
            out var collaboratorIds);

        collaboratorIds ??= Array.Empty<string>();

        var additionalIds = collaboratorIds
            .Where(x => string.IsNullOrWhiteSpace(workItem.AssignedToUserId) ||
                        !string.Equals(
                            x,
                            workItem.AssignedToUserId,
                            StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var additionalNames = additionalIds
            .Select(id =>
            {
                if (!usersById.TryGetValue(id, out var user))
                {
                    return id;
                }

                return !string.IsNullOrWhiteSpace(user.FullName)
                    ? user.FullName
                    : !string.IsNullOrWhiteSpace(user.Email)
                        ? user.Email
                        : id;
            })
            .ToList();

        return MapToDto(
            workItem,
            primaryUser,
            additionalIds,
            additionalNames);
    }

    private static WorkItemDto MapToDto(
        WorkItem workItem,
        UserDto? assignedUser,
        IReadOnlyList<string>? assignedUserIds = null,
        IReadOnlyList<string>? assignedUserNames = null)
    {
        var assignedUserName = assignedUser is null
            ? null
            : !string.IsNullOrWhiteSpace(assignedUser.FullName)
                ? assignedUser.FullName
                : assignedUser.Email;

        return new WorkItemDto
        {
            Id = workItem.Id,
            ReferenceCode = $"CRH-{workItem.Id:D6}",
            ProjectId = workItem.ProjectId,
            BoardColumnId = workItem.BoardColumnId,
            AssignedToUserId = workItem.AssignedToUserId,
            AssignedToUserName = assignedUserName,
            AssignedUserIds = assignedUserIds ?? Array.Empty<string>(),
            AssignedUserNames = assignedUserNames ?? Array.Empty<string>(),
            Title = workItem.Title,
            Description = workItem.Description,
            Type = workItem.Type,
            Priority = workItem.Priority,
            DueDate = workItem.DueDate,
            IsCompleted = workItem.IsCompleted,
            SortOrder = workItem.SortOrder
        };
    }
}



