using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Models.Experiments;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

public class ExperimentsController : Controller
{
    private readonly IExperimentService _experimentService;
    private readonly IWorkItemService _workItemService;
    private readonly IBoardService _boardService;

    public ExperimentsController(
        IExperimentService experimentService,
        IWorkItemService workItemService,
        IBoardService boardService)
    {
        _experimentService = experimentService;
        _workItemService = workItemService;
        _boardService = boardService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
    CancellationToken cancellationToken)
    {
        var experiments =
            await _experimentService.GetAllAsync(
                cancellationToken);

        var workItems =
            await _workItemService.GetAllAsync(
                cancellationToken: cancellationToken);

        var workItemNames =
            workItems.ToDictionary(
                x => x.Id,
                x => x.Title);

        var items =
            experiments
                .Select(x => new ExperimentListItemViewModel
                {
                    Id = x.Id,
                    WorkItemId = x.WorkItemId,
                    WorkItemTitle =
                        workItemNames.TryGetValue(
                            x.WorkItemId,
                            out var workItemTitle)
                            ? workItemTitle
                            : $"کار #{x.WorkItemId}",

                    Title = x.Title,
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt
                })
                .ToList();

        return View(
            new ExperimentsListViewModel
            {
                Items = items
            });
    }

    [HttpGet]
    public async Task<IActionResult> Create(
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

        return View(
            new CreateExperimentViewModel
            {
                WorkItemId = workItem.Id,
                ProjectId = projectId,
                BoardId = boardId
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateExperimentViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var workItem =
            await _workItemService.GetByIdAsync(
                model.WorkItemId,
                cancellationToken);

        if (workItem is null ||
            workItem.ProjectId != model.ProjectId)
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

        try
        {
            var experiment =
                await _experimentService.CreateAsync(
                    model.WorkItemId,
                    model.Title,
                    model.Description,
                    model.Protocol,
                    model.StartedAt,
                    model.CompletedAt,
                    model.Notes,
                    cancellationToken);

            if (experiment is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The experiment could not be created.");

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
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }
}