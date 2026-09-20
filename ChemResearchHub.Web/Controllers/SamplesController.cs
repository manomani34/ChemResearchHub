using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.Samples.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Models.Samples;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

public class SamplesController : Controller
{
    private readonly ISampleService _sampleService;
    private readonly IExperimentService _experimentService;
    private readonly IWorkItemService _workItemService;
    private readonly IBoardService _boardService;

    public SamplesController(
        ISampleService sampleService,
        IExperimentService experimentService,
        IWorkItemService workItemService,
        IBoardService boardService)
    {
        _sampleService = sampleService;
        _experimentService = experimentService;
        _workItemService = workItemService;
        _boardService = boardService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
    CancellationToken cancellationToken)
    {
        var samples =
            await _sampleService.GetAllAsync(
                cancellationToken);

        var experiments =
            await _experimentService.GetAllAsync(
                cancellationToken);

        var workItems =
            await _workItemService.GetAllAsync(
                cancellationToken: cancellationToken);

        var experimentLookup =
            experiments.ToDictionary(
                x => x.Id);

        var workItemNames =
            workItems.ToDictionary(
                x => x.Id,
                x => x.Title);

        var items =
            samples
                .Select(x =>
                {
                    experimentLookup.TryGetValue(
                        x.ExperimentId,
                        out var experiment);

                    var workItemId =
                        experiment?.WorkItemId ?? 0;

                    return new SampleListItemViewModel
                    {
                        Id = x.Id,

                        ExperimentId =
                            x.ExperimentId,

                        ExperimentTitle =
                            experiment?.Title
                            ?? $"آزمایش #{x.ExperimentId}",

                        WorkItemId =
                            workItemId,

                        WorkItemTitle =
                            workItemNames.TryGetValue(
                                workItemId,
                                out var workItemTitle)
                                ? workItemTitle
                                : "—",

                        SampleCode =
                            x.SampleCode,

                        Name =
                            x.Name,

                        SampleType =
                            x.SampleType,

                        Matrix =
                            x.Matrix,

                        CollectedAt =
                            x.CollectedAt
                    };
                })
                .ToList();

        return View(
            new SamplesListViewModel
            {
                Items = items
            });
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        int experimentId,
        int workItemId,
        int projectId,
        int boardId,
        CancellationToken cancellationToken)
    {
        var experiment =
            await _experimentService.GetByIdAsync(
                experimentId,
                cancellationToken);

        if (experiment is null ||
            experiment.WorkItemId != workItemId)
        {
            return NotFound();
        }

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
            new CreateSampleViewModel
            {
                ExperimentId = experimentId,
                WorkItemId = workItemId,
                ProjectId = projectId,
                BoardId = boardId
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateSampleViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var experiment =
            await _experimentService.GetByIdAsync(
                model.ExperimentId,
                cancellationToken);

        if (experiment is null ||
            experiment.WorkItemId != model.WorkItemId)
        {
            return NotFound();
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
            var sample =
                await _sampleService.CreateAsync(
                    model.ExperimentId,
                    model.SampleCode,
                    model.Name,
                    model.SampleType,
                    model.Matrix,
                    model.PreparationMethod,
                    model.CollectedAt,
                    model.ExternalReference,
                    model.Description,
                    model.Notes,
                    cancellationToken);

            if (sample is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The sample could not be created.");

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