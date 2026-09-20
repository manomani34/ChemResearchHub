using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.Results.Interfaces;
using ChemResearchHub.Application.Samples.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Models.Results;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

public class ResultsController : Controller
{
    private static readonly string[] AvailableStatuses =
    {
        "Pending",
        "Pass",
        "Fail",
        "Inconclusive"
    };

    private readonly IResultService _resultService;
    private readonly ISampleService _sampleService;
    private readonly IExperimentService _experimentService;
    private readonly IWorkItemService _workItemService;
    private readonly IBoardService _boardService;

    public ResultsController(
        IResultService resultService,
        ISampleService sampleService,
        IExperimentService experimentService,
        IWorkItemService workItemService,
        IBoardService boardService)
    {
        _resultService = resultService;
        _sampleService = sampleService;
        _experimentService = experimentService;
        _workItemService = workItemService;
        _boardService = boardService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
    CancellationToken cancellationToken)
    {
        var results =
            await _resultService.GetAllAsync(
                cancellationToken);

        var samples =
            await _sampleService.GetAllAsync(
                cancellationToken);

        var experiments =
            await _experimentService.GetAllAsync(
                cancellationToken);

        var workItems =
            await _workItemService.GetAllAsync(
                cancellationToken: cancellationToken);

        var sampleLookup =
            samples.ToDictionary(
                x => x.Id);

        var experimentLookup =
            experiments.ToDictionary(
                x => x.Id);

        var workItemNames =
            workItems.ToDictionary(
                x => x.Id,
                x => x.Title);

        var items =
            results
                .Select(result =>
                {
                    sampleLookup.TryGetValue(
                        result.SampleId,
                        out var sample);

                    var experimentId =
                        sample?.ExperimentId ?? 0;

                    experimentLookup.TryGetValue(
                        experimentId,
                        out var experiment);

                    var workItemId =
                        experiment?.WorkItemId ?? 0;

                    var displayValue =
                        result.NumericValue.HasValue
                            ? result.NumericValue.Value.ToString("0.######")
                            : result.TextValue;

                    return new ResultListItemViewModel
                    {
                        Id = result.Id,

                        SampleId =
                            result.SampleId,

                        SampleCode =
                            sample?.SampleCode
                            ?? $"نمونه #{result.SampleId}",

                        SampleName =
                            sample?.Name,

                        ExperimentId =
                            experimentId,

                        ExperimentTitle =
                            experiment?.Title
                            ?? $"آزمایش #{experimentId}",

                        WorkItemId =
                            workItemId,

                        WorkItemTitle =
                            workItemNames.TryGetValue(
                                workItemId,
                                out var workItemTitle)
                                ? workItemTitle
                                : "—",

                        MetricName =
                            result.MetricName,

                        NumericValue =
                            result.NumericValue,

                        TextValue =
                            result.TextValue,

                        Unit =
                            result.Unit,

                        Method =
                            result.Method,

                        Status =
                            result.Status
                    };
                })
                .ToList();

        return View(
            new ResultsListViewModel
            {
                Items = items
            });
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        int sampleId,
        int experimentId,
        int workItemId,
        int projectId,
        int boardId,
        CancellationToken cancellationToken)
    {
        var contextValid =
            await ValidateContextAsync(
                sampleId,
                experimentId,
                workItemId,
                projectId,
                boardId,
                cancellationToken);

        if (!contextValid)
        {
            return NotFound();
        }

        return View(
            new CreateResultViewModel
            {
                SampleId = sampleId,
                ExperimentId = experimentId,
                WorkItemId = workItemId,
                ProjectId = projectId,
                BoardId = boardId
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateResultViewModel model,
        CancellationToken cancellationToken)
    {
        if (!AvailableStatuses.Contains(
                model.Status,
                StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.Status),
                "The selected status is not valid.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var contextValid =
            await ValidateContextAsync(
                model.SampleId,
                model.ExperimentId,
                model.WorkItemId,
                model.ProjectId,
                model.BoardId,
                cancellationToken);

        if (!contextValid)
        {
            return NotFound();
        }

        try
        {
            var result =
                await _resultService.CreateAsync(
                    model.SampleId,
                    model.MetricName,
                    model.NumericValue,
                    model.TextValue,
                    model.Unit,
                    model.Method,
                    model.Status,
                    model.Evidence,
                    model.Notes,
                    cancellationToken);

            if (result is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The result could not be created.");

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

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        int sampleId,
        int experimentId,
        int workItemId,
        int projectId,
        int boardId,
        CancellationToken cancellationToken)
    {
        var result =
            await _resultService.GetByIdAsync(
                id,
                cancellationToken);

        if (result is null ||
            result.SampleId != sampleId)
        {
            return NotFound();
        }

        var contextValid =
            await ValidateContextAsync(
                sampleId,
                experimentId,
                workItemId,
                projectId,
                boardId,
                cancellationToken);

        if (!contextValid)
        {
            return NotFound();
        }

        return View(
            new EditResultViewModel
            {
                Id = result.Id,
                SampleId = result.SampleId,
                ExperimentId = experimentId,
                WorkItemId = workItemId,
                ProjectId = projectId,
                BoardId = boardId,
                MetricName = result.MetricName,
                NumericValue = result.NumericValue,
                TextValue = result.TextValue,
                Unit = result.Unit,
                Method = result.Method,
                Status = result.Status,
                Evidence = result.Evidence,
                Notes = result.Notes
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditResultViewModel model,
        CancellationToken cancellationToken)
    {
        if (!AvailableStatuses.Contains(
                model.Status,
                StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.Status),
                "The selected status is not valid.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingResult =
            await _resultService.GetByIdAsync(
                model.Id,
                cancellationToken);

        if (existingResult is null ||
            existingResult.SampleId != model.SampleId)
        {
            return NotFound();
        }

        var contextValid =
            await ValidateContextAsync(
                model.SampleId,
                model.ExperimentId,
                model.WorkItemId,
                model.ProjectId,
                model.BoardId,
                cancellationToken);

        if (!contextValid)
        {
            return NotFound();
        }

        try
        {
            var updated =
                await _resultService.UpdateAsync(
                    model.Id,
                    model.MetricName,
                    model.NumericValue,
                    model.TextValue,
                    model.Unit,
                    model.Method,
                    model.Status,
                    model.Evidence,
                    model.Notes,
                    cancellationToken);

            if (!updated)
            {
                return NotFound();
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

    private async Task<bool> ValidateContextAsync(
        int sampleId,
        int experimentId,
        int workItemId,
        int projectId,
        int boardId,
        CancellationToken cancellationToken)
    {
        var sample =
            await _sampleService.GetByIdAsync(
                sampleId,
                cancellationToken);

        if (sample is null ||
            sample.ExperimentId != experimentId)
        {
            return false;
        }

        var experiment =
            await _experimentService.GetByIdAsync(
                experimentId,
                cancellationToken);

        if (experiment is null ||
            experiment.WorkItemId != workItemId)
        {
            return false;
        }

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

        if (board is null ||
            board.ProjectId != projectId)
        {
            return false;
        }

        return true;
    }
}