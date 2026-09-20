using ChemResearchHub.Application.Attachments.Interfaces;
using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.DecisionLogs.Interfaces;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Application.Results.Dtos;
using ChemResearchHub.Application.Results.Interfaces;
using ChemResearchHub.Application.Samples.Dtos;
using ChemResearchHub.Application.Samples.Interfaces;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Models.WorkItems;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

public class WorkItemsController : Controller
{
    private readonly IWorkItemService _workItemService;
    private readonly IBoardService _boardService;
    private readonly IProjectService _projectService;
    private readonly IUserService _userService;
    private readonly IExperimentService _experimentService;
    private readonly ISampleService _sampleService;
    private readonly IResultService _resultService;
    private readonly IAttachmentService _attachmentService;
    private readonly IDecisionLogService _decisionLogService;

    public WorkItemsController(
    IWorkItemService workItemService,
    IBoardService boardService,
    IProjectService projectService,
    IUserService userService,
    IExperimentService experimentService,
    ISampleService sampleService,
    IResultService resultService,
    IAttachmentService attachmentService,
    IDecisionLogService decisionLogService)
    {
        _workItemService = workItemService;
        _boardService = boardService;
        _projectService = projectService;
        _userService = userService;
        _experimentService = experimentService;
        _sampleService = sampleService;
        _resultService = resultService;
        _attachmentService = attachmentService;
        _decisionLogService = decisionLogService;
    }


    [HttpGet]
    public async Task<IActionResult> Index(
    string? search,
    string status = "all",
    string? assignedToUserId = null,
    CancellationToken cancellationToken = default)
    {
        status =
            string.IsNullOrWhiteSpace(status)
                ? "all"
                : status.Trim().ToLowerInvariant();


        bool? isCompleted =
            status switch
            {
                "open" => false,
                "completed" => true,
                _ => null
            };


        var workItems =
            await _workItemService.GetAllAsync(
                search,
                isCompleted,
                assignedToUserId,
                cancellationToken);


        var users =
            await _userService.GetActiveUsersAsync(
                cancellationToken);


        var items =
            workItems
                .Select(
                    x =>
                        new WorkItemListItemViewModel
                        {
                            Id = x.Id,
                            ProjectId = x.ProjectId,
                            BoardColumnId = x.BoardColumnId,
                            Title = x.Title,
                            Description = x.Description,
                            Type = x.Type.ToString(),
                            Priority = x.Priority,
                            DueDate = x.DueDate,
                            IsCompleted = x.IsCompleted,
                            AssignedToUserName =
                                x.AssignedToUserName
                        })
                .ToList();


        var assignees =
            users
                .OrderBy(x => x.FullName)
                .Select(
                    x =>
                        new WorkItemAssigneeViewModel
                        {
                            Id = x.Id,
                            FullName =
                                string.IsNullOrWhiteSpace(x.FullName)
                                    ? x.Email ?? x.Id
                                    : x.FullName
                        })
                .ToList();


        var model =
            new WorkItemsIndexViewModel
            {
                Search = search,
                Status = status,
                AssignedToUserId = assignedToUserId,
                Items = items,
                Assignees = assignees
            };


        return View(model);
    }


    [HttpGet]
    public async Task<IActionResult> Create(
        int projectId,
        int boardId,
        int boardColumnId,
        CancellationToken cancellationToken)
    {
        var project =
            await _projectService.GetByIdAsync(
                projectId,
                cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        var board =
            await _boardService.GetByIdAsync(
                boardId,
                cancellationToken);

        if (board is null ||
            board.ProjectId != projectId)
        {
            return NotFound();
        }

        var column =
            board.Columns.FirstOrDefault(
                x => x.Id == boardColumnId);

        if (column is null)
        {
            return NotFound();
        }

        var users =
            await _userService.GetActiveUsersAsync(
                cancellationToken);

        ViewBag.Project = project;
        ViewBag.Board = board;
        ViewBag.Column = column;
        ViewBag.Users = users;

        return View(
            new CreateWorkItemViewModel
            {
                ProjectId = projectId,
                BoardId = boardId,
                BoardColumnId = boardColumnId
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateWorkItemViewModel model,
        CancellationToken cancellationToken)
    {
        var project =
            await _projectService.GetByIdAsync(
                model.ProjectId,
                cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        var board =
            await _boardService.GetByIdAsync(
                model.BoardId,
                cancellationToken);

        if (board is null ||
            board.ProjectId != model.ProjectId)
        {
            return NotFound();
        }

        var column =
            board.Columns.FirstOrDefault(
                x => x.Id == model.BoardColumnId);

        if (column is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Project = project;
            ViewBag.Board = board;
            ViewBag.Column = column;
            ViewBag.Users =
                await _userService.GetActiveUsersAsync(
                    cancellationToken);

            return View(model);
        }

        await _workItemService.CreateAsync(
            model.ProjectId,
            model.BoardColumnId,
            model.Title,
            model.Description,
            model.Type,
            model.Priority,
            model.DueDate,
            model.AssignedToUserId,
            cancellationToken);

        return RedirectToAction(
            "Index",
            "Boards",
            new
            {
                projectId = model.ProjectId,
                boardId = model.BoardId
            });
    }

    [HttpGet]
    public async Task<IActionResult> Details(
     int id,
     int boardId,
     int projectId,
     CancellationToken cancellationToken)
    {
        var workItem =
    await _workItemService.GetByIdAsync(
        id,
        cancellationToken);

        if (workItem is null)
        {
            return NotFound();
        }

        var attachments =
            await _attachmentService.GetByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        if (workItem is null)
        {
            return NotFound();
        }

        if (workItem.ProjectId != projectId)
        {
            return NotFound();
        }

        var board =
            await _boardService.GetByIdAsync(
                boardId,
                cancellationToken);

        if (board is null ||
            board.ProjectId != projectId)
        {
            return NotFound();
        }

        var column =
            board.Columns.FirstOrDefault(
                x => x.Id == workItem.BoardColumnId);

        if (column is null)
        {
            return NotFound();
        }

        var experiments =
            await _experimentService.GetByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        var samplesByExperimentId =
    new Dictionary<int, IReadOnlyList<SampleDto>>();

        foreach (var experiment in experiments)
        {
            var samples =
                await _sampleService.GetByExperimentIdAsync(
                    experiment.Id,
                    cancellationToken);

            samplesByExperimentId[experiment.Id] = samples;
        }

        var resultsBySampleId =
    new Dictionary<int, IReadOnlyList<ResultDto>>();

        foreach (var experiment in experiments)
        {
            var samples =
                samplesByExperimentId.TryGetValue(
                    experiment.Id,
                    out var experimentSamples)
                    ? experimentSamples
                    : Array.Empty<
                        ChemResearchHub.Application.Samples.Dtos.SampleDto>();

            foreach (var sample in samples)
            {
                var results =
                    await _resultService.GetBySampleIdAsync(
                        sample.Id,
                        cancellationToken);

                resultsBySampleId[sample.Id] = results;
            }
        }

        var decisionLogs =
    await _decisionLogService.GetByWorkItemIdAsync(
        workItem.Id,
        cancellationToken);

        var model =
     new WorkItemDetailsViewModel
     {
         Id = workItem.Id,
         ProjectId = workItem.ProjectId,
         BoardId = board.Id,
         BoardColumnId = column.Id,
         ColumnName = column.Name,
         Title = workItem.Title,
         Description = workItem.Description,
         Type = workItem.Type,
         Priority = workItem.Priority,
         DueDate = workItem.DueDate,
         IsCompleted = workItem.IsCompleted,
         SortOrder = workItem.SortOrder,
         AssignedToUserId = workItem.AssignedToUserId,
         AssignedToUserName = workItem.AssignedToUserName,

         Experiments = experiments,
         SamplesByExperimentId = samplesByExperimentId,
         ResultsBySampleId = resultsBySampleId,

         Attachments = attachments,

         DecisionLogs = decisionLogs,
     };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        int boardId,
        int projectId,
        CancellationToken cancellationToken)
    {
        var workItem =
            await _workItemService.GetByIdAsync(
                id,
                cancellationToken);

        if (workItem is null)
        {
            return NotFound();
        }

        if (workItem.ProjectId != projectId)
        {
            return NotFound();
        }

        var board =
            await _boardService.GetByIdAsync(
                boardId,
                cancellationToken);

        if (board is null ||
            board.ProjectId != projectId)
        {
            return NotFound();
        }

        var column =
            board.Columns.FirstOrDefault(
                x => x.Id == workItem.BoardColumnId);

        if (column is null)
        {
            return NotFound();
        }

        var users =
            await _userService.GetActiveUsersAsync(
                cancellationToken);

        ViewBag.Users = users;

        return View(
            new EditWorkItemViewModel
            {
                Id = workItem.Id,
                ProjectId = workItem.ProjectId,
                BoardId = board.Id,
                BoardColumnId = column.Id,
                Title = workItem.Title,
                Description = workItem.Description,
                Type = workItem.Type,
                Priority = workItem.Priority,
                DueDate = workItem.DueDate,
                AssignedToUserId = workItem.AssignedToUserId,
                IsCompleted = workItem.IsCompleted
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditWorkItemViewModel model,
        CancellationToken cancellationToken)
    {
        var project =
            await _projectService.GetByIdAsync(
                model.ProjectId,
                cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        var board =
            await _boardService.GetByIdAsync(
                model.BoardId,
                cancellationToken);

        if (board is null ||
            board.ProjectId != model.ProjectId)
        {
            return NotFound();
        }

        var workItem =
            await _workItemService.GetByIdAsync(
                model.Id,
                cancellationToken);

        if (workItem is null ||
            workItem.ProjectId != model.ProjectId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Users =
                await _userService.GetActiveUsersAsync(
                    cancellationToken);

            return View(model);
        }

        var updated =
            await _workItemService.UpdateAsync(
                model.Id,
                model.Title,
                model.Description,
                model.Type,
                model.Priority,
                model.DueDate,
                model.AssignedToUserId,
                cancellationToken);

        if (updated is null)
        {
            return NotFound();
        }

        if (model.IsCompleted != updated.IsCompleted)
        {
            if (model.IsCompleted)
            {
                await _workItemService.CompleteAsync(
                    model.Id,
                    cancellationToken);
            }
            else
            {
                await _workItemService.ReopenAsync(
                    model.Id,
                    cancellationToken);
            }
        }

        return RedirectToAction(
            "Index",
            "Boards",
            new
            {
                projectId = model.ProjectId,
                boardId = model.BoardId
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(
        int id,
        int boardId,
        int projectId,
        int boardColumnId,
        int sortOrder = 0,
        CancellationToken cancellationToken = default)
    {
        var moved =
            await _workItemService.MoveAsync(
                id,
                boardColumnId,
                sortOrder,
                cancellationToken);

        if (!moved)
        {
            return NotFound();
        }

        return RedirectToAction(
            "Index",
            "Boards",
            new
            {
                projectId,
                boardId
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(
        int id,
        int boardId,
        int projectId,
        CancellationToken cancellationToken)
    {
        var completed =
            await _workItemService.CompleteAsync(
                id,
                cancellationToken);

        if (!completed)
        {
            return NotFound();
        }

        return RedirectToAction(
            "Index",
            "Boards",
            new
            {
                projectId,
                boardId
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reopen(
        int id,
        int boardId,
        int projectId,
        CancellationToken cancellationToken)
    {
        var reopened =
            await _workItemService.ReopenAsync(
                id,
                cancellationToken);

        if (!reopened)
        {
            return NotFound();
        }

        return RedirectToAction(
            "Index",
            "Boards",
            new
            {
                projectId,
                boardId
            });
    }
}