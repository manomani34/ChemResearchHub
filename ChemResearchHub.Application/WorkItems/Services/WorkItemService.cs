using ChemResearchHub.Application.AuditLogs.Interfaces;
using ChemResearchHub.Application.Boards.Repositories;
using ChemResearchHub.Application.Users.Dtos;
using ChemResearchHub.Application.Users.Interfaces;
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

    public WorkItemService(
        IWorkItemRepository workItemRepository,
        IBoardRepository boardRepository,
        IUserService userService,
        IWorkItemTransitionRepository workItemTransitionRepository,
        IAuditLogService auditLogService)
    {
        _workItemRepository = workItemRepository;
        _boardRepository = boardRepository;
        _userService = userService;
        _workItemTransitionRepository = workItemTransitionRepository;
        _auditLogService = auditLogService;
    }

    public async Task<IReadOnlyList<WorkItemDto>> GetByBoardColumnIdAsync(
        int boardColumnId,
        CancellationToken cancellationToken = default)
    {
        var workItems =
            await _workItemRepository.GetByBoardColumnIdAsync(
                boardColumnId,
                cancellationToken);

        var users =
            await _userService.GetActiveUsersAsync(
                cancellationToken);

        var usersById =
            users.ToDictionary(
                x => x.Id,
                x => x);

        return workItems
            .Select(x => MapToDto(x, usersById))
            .ToList();
    }

    public async Task<IReadOnlyList<WorkItemDto>> GetAllAsync(
        string? search = null,
        bool? isCompleted = null,
        string? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        var workItems =
            await _workItemRepository.GetAllAsync(
                search,
                isCompleted,
                assignedToUserId,
                cancellationToken);

        var users =
            await _userService.GetActiveUsersAsync(
                cancellationToken);

        var usersById =
            users.ToDictionary(
                x => x.Id,
                x => x);

        return workItems
            .Select(
                x =>
                    MapToDto(
                        x,
                        usersById))
            .ToList();
    }

    public async Task<WorkItemDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var workItem =
            await _workItemRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (workItem is null)
        {
            return null;
        }

        UserDto? assignedUser = null;

        if (!string.IsNullOrWhiteSpace(
                workItem.AssignedToUserId))
        {
            assignedUser =
                await _userService.GetByIdAsync(
                    workItem.AssignedToUserId,
                    cancellationToken);
        }

        return MapToDto(
            workItem,
            assignedUser);
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
        CancellationToken cancellationToken = default)
    {
        var workItem =
            await _workItemRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (workItem is null)
        {
            return null;
        }

        await ValidateAssignedUserAsync(
            assignedToUserId,
            cancellationToken);

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
            workItem.AssignTo(
                assignedToUserId);
        }

        await _workItemRepository.SaveChangesAsync(
            cancellationToken);

        await _auditLogService.AddAsync(
            changedByUserId,
            "Updated",
            "WorkItem",
            workItem.Id,
            $"Work Item '{workItem.Title}' was updated.",
            cancellationToken: cancellationToken);

        UserDto? assignedUser = null;

        if (!string.IsNullOrWhiteSpace(
                workItem.AssignedToUserId))
        {
            assignedUser =
                await _userService.GetByIdAsync(
                    workItem.AssignedToUserId,
                    cancellationToken);
        }

        return MapToDto(
            workItem,
            assignedUser);
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
        CancellationToken cancellationToken = default)
    {
        var board =
            await _boardRepository.GetByColumnIdAsync(
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

        var existingItems =
            await _workItemRepository.GetByBoardColumnIdAsync(
                boardColumnId,
                cancellationToken);

        var targetColumn =
            board.Columns.FirstOrDefault(
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

        var nextSortOrder =
            existingItems.Count;

        var workItem =
            new WorkItem(
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

        if (!string.IsNullOrWhiteSpace(
                assignedToUserId))
        {
            workItem.AssignTo(
                assignedToUserId);
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

        await _workItemRepository.SaveChangesAsync(
            cancellationToken);

        await _auditLogService.AddAsync(
            changedByUserId,
            "Created",
            "WorkItem",
            workItem.Id,
            $"Work Item '{workItem.Title}' was created.",
            cancellationToken: cancellationToken);

        UserDto? assignedUser = null;

        if (!string.IsNullOrWhiteSpace(
                workItem.AssignedToUserId))
        {
            assignedUser =
                await _userService.GetByIdAsync(
                    workItem.AssignedToUserId,
                    cancellationToken);
        }

        return MapToDto(
            workItem,
            assignedUser);
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

        return true;
    }

    public async Task<bool> ReopenAsync(
        int id,
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

    private static WorkItemDto MapToDto(
        WorkItem workItem,
        IReadOnlyDictionary<string, UserDto> usersById)
    {
        UserDto? assignedUser = null;

        if (!string.IsNullOrWhiteSpace(
                workItem.AssignedToUserId))
        {
            usersById.TryGetValue(
                workItem.AssignedToUserId,
                out assignedUser);
        }

        return MapToDto(
            workItem,
            assignedUser);
    }

    private static WorkItemDto MapToDto(
        WorkItem workItem,
        UserDto? assignedUser)
    {
        var assignedUserName =
            assignedUser is null
                ? null
                : !string.IsNullOrWhiteSpace(
                    assignedUser.FullName)
                    ? assignedUser.FullName
                    : assignedUser.Email;

        return new WorkItemDto
        {
            Id = workItem.Id,
            ProjectId = workItem.ProjectId,
            BoardColumnId = workItem.BoardColumnId,
            AssignedToUserId = workItem.AssignedToUserId,
            AssignedToUserName = assignedUserName,
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