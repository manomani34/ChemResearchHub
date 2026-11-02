
using ChemResearchHub.Application.AuditLogs.Interfaces;
using ChemResearchHub.Application.Boards.Repositories;
using ChemResearchHub.Application.Configuration;
using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Application.Notifications.Models;
using ChemResearchHub.Application.Tags.Repositories;
using ChemResearchHub.Application.Users.Dtos;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.WorkItemAssignments.Repositories;
using ChemResearchHub.Application.WorkItems.Dtos;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Application.WorkItems.Repositories;
using ChemResearchHub.Application.WorkItemTransitions.Repositories;
using ChemResearchHub.Domain.Entities.WorkItem;
using ChemResearchHub.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChemResearchHub.Application.WorkItems.Services;

public class WorkItemService : IWorkItemService
{
    private readonly IWorkItemRepository _workItemRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly IUserService _userService;
    private readonly IWorkItemTransitionRepository _workItemTransitionRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IWorkItemAssignmentRepository _workItemAssignmentRepository;
    private readonly IWorkItemTagRepository _workItemTagRepository;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly ILogger<WorkItemService> _logger;
    private readonly ApplicationOptions _applicationOptions;

    public WorkItemService(
        IWorkItemRepository workItemRepository,
        IBoardRepository boardRepository,
        IUserService userService,
        IWorkItemTransitionRepository workItemTransitionRepository,
        IAuditLogService auditLogService,
        IWorkItemAssignmentRepository workItemAssignmentRepository,
        IWorkItemTagRepository workItemTagRepository,
        INotificationDispatcher notificationDispatcher,
        ILogger<WorkItemService> logger,
        IOptions<ApplicationOptions> applicationOptions)
    {
        _workItemRepository = workItemRepository;
        _boardRepository = boardRepository;
        _userService = userService;
        _workItemTransitionRepository = workItemTransitionRepository;
        _auditLogService = auditLogService;
        _workItemAssignmentRepository = workItemAssignmentRepository;
        _workItemTagRepository = workItemTagRepository;
        _notificationDispatcher = notificationDispatcher;
        _logger = logger;
        _applicationOptions = applicationOptions.Value;
    }

    #region Notifications

    private async Task NotifyAssignedUserAsync(
    string userId,
    string workItemTitle,
    int workItemId,
    int boardId,
    int projectId,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "Assignment notification skipped because UserId is empty. WorkItemId: {WorkItemId}",
                workItemId);
            return;
        }

        userId = userId.Trim();

        // Get the assigned user's display name.
        var users = await _userService.GetAllAsync(cancellationToken);

        var assignedUser = users.FirstOrDefault(
            x => string.Equals(x.Id, userId, StringComparison.Ordinal));

        var assignedUserName =
            !string.IsNullOrWhiteSpace(assignedUser?.FullName)
                ? assignedUser.FullName
                : !string.IsNullOrWhiteSpace(assignedUser?.Email)
                    ? assignedUser.Email
                    : userId;

        _logger.LogInformation(
            "Starting assignment notification. WorkItemId: {WorkItemId}, UserId: {UserId}, UserName: {UserName}",
            workItemId,
            userId,
            assignedUserName);

        var baseUrl = _applicationOptions.PublicBaseUrl.TrimEnd('/');

        var actionUrl =
            $"{baseUrl}/Research/WorkItems/Details/{workItemId}" +
            $"?boardId={boardId}&projectId={projectId}";

        var notification = new NotificationMessage
        {
            UserId = userId,
            Type = "WorkItemAssigned",
            Title = "انتساب فعالیت جدید",
            Message = $"فعالیت «{workItemTitle}» به {assignedUserName} واگذار شد.",
            ActionUrl = actionUrl
        };

        // Dispatch In-App notification.
        try
        {
            var inAppResult = await _notificationDispatcher.DispatchInAppAsync(
                notification,
                cancellationToken);

            LogChannelResults(inAppResult, workItemId, userId);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to dispatch In-App assignment notification. WorkItemId: {WorkItemId}",
                workItemId);
        }

        // Dispatch Bale notification.
        try
        {
            var baleResult = await _notificationDispatcher.DispatchBaleAsync(
                notification,
                cancellationToken);

            LogChannelResults(baleResult, workItemId, userId);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to dispatch Bale assignment notification. WorkItemId: {WorkItemId}",
                workItemId);
        }
    }

    private async Task NotifyWorkItemEventToBaleAsync(
    WorkItem workItem,
    string notificationType,
    string notificationTitle,
    string eventMessage,
    string? changedByUserId,
    CancellationToken cancellationToken)
    {
        try
        {
            if (!workItem.BoardColumnId.HasValue)
            {
                _logger.LogWarning(
                    "Bale event notification skipped because Work Item has no BoardColumnId. WorkItemId: {WorkItemId}, Event: {Event}",
                    workItem.Id,
                    notificationType);

                return;
            }

            var board = await _boardRepository.GetByColumnIdAsync(
                workItem.BoardColumnId.Value,
                cancellationToken);

            if (board is null)
            {
                _logger.LogWarning(
                    "Bale event notification skipped because Board was not found. WorkItemId: {WorkItemId}, Event: {Event}",
                    workItem.Id,
                    notificationType);

                return;
            }

            var users = await _userService.GetAllAsync(cancellationToken);

            var usersById = users.ToDictionary(
                x => x.Id,
                x => x);

            var actorName = ResolveUserName(
                changedByUserId,
                usersById);

            var assignedUserName = ResolveUserName(
                workItem.AssignedToUserId,
                usersById);

            var collaboratorIds =
                await _workItemAssignmentRepository.GetUserIdsByWorkItemIdAsync(
                    workItem.Id,
                    cancellationToken);

            var additionalCollaboratorNames = collaboratorIds
                .Where(userId =>
                    !string.IsNullOrWhiteSpace(userId) &&
                    !string.Equals(
                        userId,
                        workItem.AssignedToUserId,
                        StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .Select(userId => ResolveUserName(userId, usersById))
                .ToList();

            var assignmentLines = new List<string>();

            if (!string.IsNullOrWhiteSpace(assignedUserName))
            {
                assignmentLines.Add(
                    $"👤 **Assigned To:** {assignedUserName}");
            }
            else
            {
                assignmentLines.Add(
                    "👤 **Assigned To:** تعیین نشده");
            }

            if (additionalCollaboratorNames.Count > 0)
            {
                assignmentLines.Add(
                    $"👥 **Additional Collaborators:** {string.Join("، ", additionalCollaboratorNames)}");
            }
            else
            {
                assignmentLines.Add(
                    "👥 **Additional Collaborators:** ندارد");
            }

            var baseUrl = _applicationOptions.PublicBaseUrl.TrimEnd('/');

            var actionUrl =
                $"{baseUrl}/Research/WorkItems/Details/{workItem.Id}" +
                $"?boardId={board.Id}&projectId={workItem.ProjectId}";

            var message =
                $"{eventMessage}\n\n" +
                $"📌 **عنوان فعالیت:** {workItem.Title}\n" +
                $"🧑‍💻 **انجام‌دهنده:** {actorName}\n\n" +
                string.Join("\n", assignmentLines);

            var notification = new NotificationMessage
            {
                // BaleNotificationChannel uses GroupChatId and does not use UserId.
                UserId = string.Empty,
                Type = notificationType,
                Title = notificationTitle,
                Message = message,
                ActionUrl = actionUrl
            };

            var result = await _notificationDispatcher.DispatchBaleAsync(
                notification,
                cancellationToken);

            LogChannelResults(
                result,
                workItem.Id,
                "GROUP");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send Bale Work Item event notification. WorkItemId: {WorkItemId}, Event: {Event}",
                workItem.Id,
                notificationType);
        }
    }

    private static string ResolveUserName(
        string? userId,
        IReadOnlyDictionary<string, UserDto> usersById)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return "نامشخص";
        }

        if (!usersById.TryGetValue(userId, out var user))
        {
            return userId;
        }

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            return user.FullName;
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            return user.Email;
        }

        return userId;
    }

    private void LogChannelResults(
        NotificationDispatchResult dispatchResult,
        int workItemId,
        string userId)
    {
        if (dispatchResult.Channels.Count == 0)
        {
            _logger.LogWarning(
                "Notification dispatcher returned no channel results. WorkItemId: {WorkItemId}, UserId: {UserId}",
                workItemId,
                userId);

            return;
        }

        foreach (var result in dispatchResult.Channels)
        {
            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "Notification sent successfully. Channel: {Channel}, WorkItemId: {WorkItemId}, UserId: {UserId}",
                    result.Channel,
                    workItemId,
                    userId);
            }
            else
            {
                _logger.LogError(
                    "Notification failed. Channel: {Channel}, WorkItemId: {WorkItemId}, UserId: {UserId}, Error: {Error}",
                    result.Channel,
                    workItemId,
                    userId,
                    result.Error);
            }
        }
    }

    #endregion

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

        var tags = await _workItemTagRepository.GetTagsByWorkItemIdsAsync(
            workItems.Select(x => x.Id).ToArray(),
            cancellationToken);

        return workItems
            .Select(x => MapToDto(x, usersById, assignments, tags))
            .ToList();
    }

    public async Task<IReadOnlyList<WorkItemDto>> GetAllAsync(
        string? search = null,
        bool? isCompleted = null,
        string? assignedToUserId = null,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<int>? tagIds = null)
    {
        var workItems = await _workItemRepository.GetAllAsync(
            search,
            isCompleted,
            assignedToUserId,
            cancellationToken,
            tagIds: tagIds);

        var users = await _userService.GetAllAsync(cancellationToken);
        var usersById = users.ToDictionary(x => x.Id, x => x);

        var assignments = await _workItemAssignmentRepository
            .GetUserIdsByWorkItemIdsAsync(
                workItems.Select(x => x.Id).ToArray(),
                cancellationToken);

        var tags = await _workItemTagRepository.GetTagsByWorkItemIdsAsync(
            workItems.Select(x => x.Id).ToArray(),
            cancellationToken);

        return workItems
            .Select(x => MapToDto(x, usersById, assignments, tags))
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

        var tags = await _workItemTagRepository.GetTagsByWorkItemIdsAsync(
            new[] { id },
            cancellationToken);

        return MapToDto(
            workItem,
            usersById,
            new Dictionary<int, IReadOnlyList<string>>
            {
                [id] = assignments
            },
            tags);
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
    IReadOnlyCollection<string>? assignedUserIds = null,
    IReadOnlyCollection<int>? tagIds = null,
    bool suppressBaleEditNotification = false)
    {
        var workItem = await _workItemRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (workItem is null)
        {
            return null;
        }

        // Capture the previous values before updating the entity.
        var previousTitle = workItem.Title;
        var previousDescription = workItem.Description;
        var previousType = workItem.Type;
        var previousPriority = workItem.Priority;
        var previousDueDate = workItem.DueDate;

        var previousAssignedToUserId =
            NormalizeUserId(workItem.AssignedToUserId);

        await ValidateAssignedUserAsync(
            assignedToUserId,
            cancellationToken);

        // Preserve existing collaborators when the caller omits the list.
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

        // Update the main Work Item properties.
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

        var newAssignedToUserId =
            NormalizeUserId(workItem.AssignedToUserId);

        // Detect actual changes in the main Work Item properties.
        var hasEditChanges =
            !string.Equals(
                previousTitle,
                workItem.Title,
                StringComparison.Ordinal) ||

            !string.Equals(
                previousDescription,
                workItem.Description,
                StringComparison.Ordinal) ||

            previousType != workItem.Type ||
            previousPriority != workItem.Priority ||
            previousDueDate != workItem.DueDate ||

            !string.Equals(
                previousAssignedToUserId,
                newAssignedToUserId,
                StringComparison.Ordinal);

        await _workItemAssignmentRepository.ReplaceAssignmentsAsync(
            workItem.Id,
            collaboratorIds,
            changedByUserId,
            cancellationToken);

        if (tagIds is not null)
        {
            await _workItemTagRepository.ReplaceActiveTagsAsync(
                workItem.Id,
                tagIds,
                cancellationToken);
        }

        // Both repositories use the same scoped DbContext.
        await _workItemRepository.SaveChangesAsync(
            cancellationToken);

        await _auditLogService.AddAsync(
            changedByUserId,
            "Updated",
            "WorkItem",
            workItem.Id,
            $"Work Item '{workItem.Title}' was updated.",
            cancellationToken: cancellationToken);

        // Notify only when the primary assignee changes to a new user.
        var hasNewAssignee =
            !string.IsNullOrWhiteSpace(newAssignedToUserId) &&
            !string.Equals(
                previousAssignedToUserId,
                newAssignedToUserId,
                StringComparison.Ordinal);

        if (hasNewAssignee)
        {
            _logger.LogInformation(
                "Work Item assignee changed. WorkItemId: {WorkItemId}, PreviousUserId: {PreviousUserId}, NewUserId: {NewUserId}",
                workItem.Id,
                previousAssignedToUserId,
                newAssignedToUserId);

            if (!workItem.BoardColumnId.HasValue)
            {
                _logger.LogWarning(
                    "Assignment notification skipped because Work Item has no BoardColumnId. WorkItemId: {WorkItemId}",
                    workItem.Id);
            }
            else
            {
                var board = await _boardRepository.GetByColumnIdAsync(
                    workItem.BoardColumnId.Value,
                    cancellationToken);

                if (board is null)
                {
                    _logger.LogWarning(
                        "Assignment notification skipped because Board was not found. WorkItemId: {WorkItemId}, BoardColumnId: {BoardColumnId}",
                        workItem.Id,
                        workItem.BoardColumnId.Value);
                }
                else
                {
                    await NotifyAssignedUserAsync(
                        newAssignedToUserId!,
                        workItem.Title,
                        workItem.Id,
                        board.Id,
                        workItem.ProjectId,
                        cancellationToken);
                }
            }
        }
        else
        {
            _logger.LogInformation(
                "No assignment notification required for Work Item update. WorkItemId: {WorkItemId}",
                workItem.Id);
        }

        // Send a general Bale notification only for an actual edit.
        // Suppress it when this update is part of a Complete/Reopen operation.
        if (hasEditChanges && !suppressBaleEditNotification)
        {
            await NotifyWorkItemEventToBaleAsync(
                workItem,
                "WorkItemEdited",
                "ویرایش فعالیت",
                "✏️ یک فعالیت ویرایش شد.",
                changedByUserId,
                cancellationToken);
        }
        else if (suppressBaleEditNotification)
        {
            _logger.LogInformation(
                "Bale edit notification suppressed because the Work Item status is also changing. WorkItemId: {WorkItemId}",
                workItem.Id);
        }
        else
        {
            _logger.LogInformation(
                "No actual Work Item changes detected. Bale edit notification skipped. WorkItemId: {WorkItemId}",
                workItem.Id);
        }

        var users = await _userService.GetAllAsync(
            cancellationToken);

        var usersById = users.ToDictionary(
            x => x.Id,
            x => x);

        var savedCollaboratorIds =
            await _workItemAssignmentRepository.GetUserIdsByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        var savedTags = await _workItemTagRepository.GetTagsByWorkItemIdsAsync(
            new[] { workItem.Id },
            cancellationToken);

        return MapToDto(
            workItem,
            usersById,
            new Dictionary<int, IReadOnlyList<string>>
            {
                [workItem.Id] = savedCollaboratorIds
            },
            savedTags);
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
        IReadOnlyCollection<string>? assignedUserIds = null,
        IReadOnlyCollection<int>? tagIds = null)
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

        if (tagIds is { Count: > 0 })
        {
            await _workItemTagRepository.ReplaceActiveTagsAsync(
                workItem.Id,
                tagIds,
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

        // Notify only the primary assignee.
        // Additional collaborators do not receive assignment notifications.
        var normalizedAssigneeId =
            NormalizeUserId(assignedToUserId);

        if (!string.IsNullOrWhiteSpace(normalizedAssigneeId))
        {
            _logger.LogInformation(
                "New Work Item created with Assigned To. WorkItemId: {WorkItemId}, UserId: {UserId}",
                workItem.Id,
                normalizedAssigneeId);

            await NotifyAssignedUserAsync(normalizedAssigneeId,
                                          workItem.Title,
                                          workItem.Id,
                                          board.Id,
                                          projectId,
                                          cancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "Work Item created without Assigned To. No assignment notification will be sent. WorkItemId: {WorkItemId}",
                workItem.Id);
        }

        var users = await _userService.GetAllAsync(cancellationToken);
        var usersById = users.ToDictionary(x => x.Id, x => x);

        var savedCollaboratorIds =
            await _workItemAssignmentRepository.GetUserIdsByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        var savedTags = await _workItemTagRepository.GetTagsByWorkItemIdsAsync(
            new[] { workItem.Id },
            cancellationToken);

        return MapToDto(
            workItem,
            usersById,
            new Dictionary<int, IReadOnlyList<string>>
            {
                [workItem.Id] = savedCollaboratorIds
            },
            savedTags);
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

        string? currentColumnName = null;

        if (currentColumnId.HasValue)
        {
            var sourceBoard = await _boardRepository.GetByColumnIdAsync(
                currentColumnId.Value,
                cancellationToken);

            currentColumnName = sourceBoard?.Columns
                .FirstOrDefault(x => x.Id == currentColumnId.Value)
                ?.Name;
        }

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

        if (isMovingToAnotherColumn)
        {
            var targetColumnName = targetColumn.Name;

            var moveMessage =
                $"🚚 فعالیت از ستون «{currentColumnName ?? "نامشخص"}» " +
                $"به ستون «{targetColumnName}» منتقل شد.";

            if (isDoneColumn)
            {
                moveMessage += "\nوضعیت فعالیت به تکمیل‌شده تغییر کرد.";
            }
            else if (string.Equals(
                         currentColumnName?.Trim(),
                         "Done",
                         StringComparison.OrdinalIgnoreCase))
            {
                moveMessage += "\nوضعیت فعالیت به باز تغییر کرد.";
            }

            await NotifyWorkItemEventToBaleAsync(
                workItem,
                "WorkItemMoved",
                "انتقال فعالیت",
                moveMessage,
                changedByUserId,
                cancellationToken);
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

        if (workItem.IsCompleted)
        {
            return true;
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

        await NotifyWorkItemEventToBaleAsync(
    workItem,
    "WorkItemCompleted",
    "تکمیل فعالیت",
    "✅ این فعالیت تکمیل شد.",
    changedByUserId,
    cancellationToken);

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

        if (!workItem.IsCompleted)
        {
            return true;
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

        await NotifyWorkItemEventToBaleAsync(
    workItem,
    "WorkItemReopened",
    "بازگشایی مجدد فعالیت",
    "🔄 این فعالیت دوباره باز شد.",
    changedByUserId,
    cancellationToken);

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
                assignedToUserId.Trim(),
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
        var normalizedPrimaryUserId =
            NormalizeUserId(primaryUserId);

        var normalizedIds = (assignedUserIds ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Where(x =>
                string.IsNullOrWhiteSpace(normalizedPrimaryUserId) ||
                !string.Equals(
                    x,
                    normalizedPrimaryUserId,
                    StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        foreach (var userId in normalizedIds)
        {
            await ValidateAssignedUserAsync(
                userId,
                cancellationToken);
        }

        return normalizedIds;
    }

    private static string? NormalizeUserId(string? userId)
    {
        return string.IsNullOrWhiteSpace(userId)
            ? null
            : userId.Trim();
    }

    private static WorkItemDto MapToDto(
        WorkItem workItem,
        IReadOnlyDictionary<string, UserDto> usersById,
        IReadOnlyDictionary<int, IReadOnlyList<string>> assignmentsByWorkItemId,
        IReadOnlyDictionary<int, IReadOnlyList<ChemResearchHub.Application.Tags.Dtos.TagDto>>? tagsByWorkItemId = null)
    {
        usersById.TryGetValue(
            workItem.AssignedToUserId ?? string.Empty,
            out var primaryUser);

        assignmentsByWorkItemId.TryGetValue(
            workItem.Id,
            out var collaboratorIds);

        collaboratorIds ??= Array.Empty<string>();

        var additionalIds = collaboratorIds
            .Where(x =>
                string.IsNullOrWhiteSpace(workItem.AssignedToUserId) ||
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
            additionalNames,
            tagsByWorkItemId is not null &&
            tagsByWorkItemId.TryGetValue(workItem.Id, out var itemTags)
                ? itemTags
                : Array.Empty<ChemResearchHub.Application.Tags.Dtos.TagDto>());
    }

    private static WorkItemDto MapToDto(
        WorkItem workItem,
        UserDto? assignedUser,
        IReadOnlyList<string>? assignedUserIds = null,
        IReadOnlyList<string>? assignedUserNames = null,
        IReadOnlyList<ChemResearchHub.Application.Tags.Dtos.TagDto>? tags = null)
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
            Tags = tags ?? Array.Empty<ChemResearchHub.Application.Tags.Dtos.TagDto>(),
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
