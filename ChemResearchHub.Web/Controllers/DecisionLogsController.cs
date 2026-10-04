using System.Security.Claims;
using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.DecisionLogs.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Models.DecisionLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

[Authorize]
public class DecisionLogsController : Controller
{
    private static readonly string[] AvailableDecisionTypes =
    {
        "Scientific",
        "Validation",
        "Review",
        "Approval",
        "Rejection",
        "Other"
    };

    private readonly IDecisionLogService _decisionLogService;
    private readonly IWorkItemService _workItemService;
    private readonly IBoardService _boardService;

    public DecisionLogsController(
        IDecisionLogService decisionLogService,
        IWorkItemService workItemService,
        IBoardService boardService)
    {
        _decisionLogService = decisionLogService;
        _workItemService = workItemService;
        _boardService = boardService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        CancellationToken cancellationToken)
    {
        var logs =
            await _decisionLogService.GetAllAsync(
                cancellationToken);

        var workItems =
            await _workItemService.GetAllAsync(
                cancellationToken: cancellationToken);

        var workItemNames =
            workItems.ToDictionary(
                x => x.Id,
                x => x.Title);

        var items =
            logs
                .Select(x => new DecisionLogListItemViewModel
                {
                    Id = x.Id,

                    ReferenceCode = x.ReferenceCode,

                    WorkItemId = x.WorkItemId,

                    WorkItemTitle =
                        workItemNames.TryGetValue(
                            x.WorkItemId,
                            out var workItemTitle)
                            ? workItemTitle
                            : $"کار #{x.WorkItemId}",

                    DecisionType =
                        x.DecisionType,

                    Decision =
                        x.Decision,

                    Rationale =
                        x.Rationale,

                    Evidence =
                        x.Evidence,

                    CreatedByUserId =
                        x.CreatedByUserId,

                    CreatedAt =
                        x.CreatedAt
                })
                .ToList();

        return View(
            new DecisionLogsListViewModel
            {
                Items = items
            });
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var logs =
            await _decisionLogService.GetAllAsync(
                cancellationToken);

        var decision =
            logs.FirstOrDefault(
                x => x.Id == id);

        if (decision is null)
        {
            return NotFound();
        }

        var workItem =
            await _workItemService.GetByIdAsync(
                decision.WorkItemId,
                cancellationToken);

        if (workItem is null)
        {
            return NotFound();
        }

        /*
         * WorkItemDto مستقیماً BoardId ندارد.
         * بنابراین Board مربوط به Work Item را
         * از روی BoardColumnId پیدا می‌کنیم.
         */
        var boards =
            await _boardService.GetByProjectIdAsync(
                workItem.ProjectId,
                cancellationToken);

        var board =
            boards.FirstOrDefault(
                x => x.Columns.Any(
                    column =>
                        column.Id == workItem.BoardColumnId));

        if (board is null)
        {
            return NotFound();
        }

        var model =
            new DecisionLogDetailsViewModel
            {
                Id = decision.Id,

                ReferenceCode =
                    decision.ReferenceCode,

                WorkItemId =
                    workItem.Id,

                WorkItemReferenceCode =
                    $"CRH-{workItem.Id:D6}",

                WorkItemTitle =
                    workItem.Title,

                ProjectId =
                    workItem.ProjectId,

                BoardId =
                    board.Id,

                DecisionType =
                    decision.DecisionType,

                Decision =
                    decision.Decision,

                Rationale =
                    decision.Rationale,

                Evidence =
                    decision.Evidence,

                CreatedByUserId =
                    decision.CreatedByUserId,

                CreatedAt =
                    decision.CreatedAt
            };

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Researcher")]
    public async Task<IActionResult> Create(
        int workItemId,
        int projectId,
        int boardId,
        CancellationToken cancellationToken)
    {
        if (!await IsValidContextAsync(
                workItemId,
                projectId,
                boardId,
                cancellationToken))
        {
            return NotFound();
        }

        return View(
            new CreateDecisionLogViewModel
            {
                WorkItemId = workItemId,
                ProjectId = projectId,
                BoardId = boardId,
                DecisionType = "Scientific"
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Researcher")]
    public async Task<IActionResult> Create(
        CreateDecisionLogViewModel model,
        CancellationToken cancellationToken)
    {
        if (!AvailableDecisionTypes.Contains(
                model.DecisionType,
                StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.DecisionType),
                "The selected decision type is not valid.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await IsValidContextAsync(
                model.WorkItemId,
                model.ProjectId,
                model.BoardId,
                cancellationToken))
        {
            return NotFound();
        }

        try
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            var log =
                await _decisionLogService.CreateAsync(
                    model.WorkItemId,
                    model.DecisionType,
                    model.Decision,
                    model.Rationale,
                    model.Evidence,
                    userId,
                    cancellationToken);

            if (log is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The decision log could not be created.");

                return View(model);
            }

            return RedirectToAction(
                "Details",
                "WorkItems",
                new
                {
                    id = model.WorkItemId,
                    projectId = model.ProjectId,
                    boardId = model.BoardId
                });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }

    private async Task<bool> IsValidContextAsync(
        int workItemId,
        int projectId,
        int boardId,
        CancellationToken cancellationToken)
    {
        var workItem =
            await _workItemService.GetByIdAsync(
                workItemId,
                cancellationToken);

        if (workItem is null ||
            workItem.ProjectId != projectId)
        {
            return false;
        }

        var board =
            await _boardService.GetByIdAsync(
                boardId,
                cancellationToken);

        return board is not null &&
               board.ProjectId == projectId;
    }
}