using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.DecisionLogs.Interfaces;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Application.ResearchReviews.Interfaces;
using ChemResearchHub.Application.Results.Interfaces;
using ChemResearchHub.Application.Samples.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Models.Samples;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

[Authorize]
public class SamplesController : Controller
{
    private readonly ISampleService _sampleService;
    private readonly IExperimentService _experimentService;
    private readonly IWorkItemService _workItemService;
    private readonly IBoardService _boardService;
    private readonly IProjectService _projectService;
    private readonly IResultService _resultService;
    private readonly IResearchReviewService _researchReviewService;
    private readonly IDecisionLogService _decisionLogService;

    public SamplesController(
        ISampleService sampleService,
        IExperimentService experimentService,
        IWorkItemService workItemService,
        IBoardService boardService,
        IProjectService projectService,
        IResultService resultService,
        IResearchReviewService researchReviewService,
        IDecisionLogService decisionLogService)
    {
        _sampleService = sampleService;
        _experimentService = experimentService;
        _workItemService = workItemService;
        _boardService = boardService;
        _projectService = projectService;
        _resultService = resultService;
        _researchReviewService = researchReviewService;
        _decisionLogService = decisionLogService;
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
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var sample =
            await _sampleService.GetByIdAsync(
                id,
                cancellationToken);

        if (sample is null)
        {
            return NotFound();
        }

        var experiment =
            await _experimentService.GetByIdAsync(
                sample.ExperimentId,
                cancellationToken);

        if (experiment is null)
        {
            return NotFound();
        }

        var workItem =
            await _workItemService.GetByIdAsync(
                experiment.WorkItemId,
                cancellationToken);

        if (workItem is null)
        {
            return NotFound();
        }

        var project =
            await _projectService.GetByIdAsync(
                workItem.ProjectId,
                cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        /*
         * =========================================================
         * Research Timeline
         * =========================================================
         */

        var timeline =
            new List<SampleTimelineItemViewModel>();

        // ---------------------------------------------------------
        // Sample
        // ---------------------------------------------------------

        if (sample.CollectedAt.HasValue)
        {
            timeline.Add(
                new SampleTimelineItemViewModel
                {
                    EventType = "Sample",
                    ReferenceCode = $"SMP-{sample.Id:D6}",
                    Title = "نمونه دریافت / ثبت شد",
                    Description = sample.Name,
                    OccurredAt = sample.CollectedAt,
                    Icon = "🧪"
                });
        }

        // ---------------------------------------------------------
        // Experiment
        // ---------------------------------------------------------

        if (experiment.StartedAt.HasValue)
        {
            timeline.Add(
                new SampleTimelineItemViewModel
                {
                    EventType = "Experiment",
                    ReferenceCode = $"EXP-{experiment.Id:D6}",
                    Title = "آزمایش شروع شد",
                    Description = experiment.Title,
                    OccurredAt = experiment.StartedAt,
                    Icon = "🔬"
                });
        }

        if (experiment.CompletedAt.HasValue)
        {
            timeline.Add(
                new SampleTimelineItemViewModel
                {
                    EventType = "Experiment",
                    ReferenceCode = $"EXP-{experiment.Id:D6}",
                    Title = "آزمایش تکمیل شد",
                    Description = experiment.Title,
                    OccurredAt = experiment.CompletedAt,
                    Icon = "✅"
                });
        }

        // ---------------------------------------------------------
        // Results
        // ---------------------------------------------------------

        var results =
            await _resultService.GetBySampleIdAsync(
                sample.Id,
                cancellationToken);

        foreach (var result in results)
        {
            var resultValue =
                result.NumericValue.HasValue
                    ? $"{result.NumericValue.Value:0.######} {result.Unit}".Trim()
                    : result.TextValue;

            timeline.Add(
                new SampleTimelineItemViewModel
                {
                    EventType = "Result",
                    ReferenceCode = result.ReferenceCode,
                    Title = $"نتیجه ثبت شد: {result.MetricName}",
                    Description =
                        string.IsNullOrWhiteSpace(resultValue)
                            ? result.Status
                            : $"{resultValue} — {result.Status}",
                    OccurredAt = null,
                    Icon = "📊"
                });
        }

        // ---------------------------------------------------------
        // Research Reviews
        // ---------------------------------------------------------

        var reviews =
            await _researchReviewService.GetByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        foreach (var review in reviews)
        {
            timeline.Add(
                new SampleTimelineItemViewModel
                {
                    EventType = "Review",
                    ReferenceCode = review.ReferenceCode,
                    Title = "درخواست بازبینی پژوهشی ثبت شد",
                    Description =
                        string.IsNullOrWhiteSpace(review.ReviewerUserName)
                            ? $"وضعیت: {review.Status}"
                            : $"Reviewer: {review.ReviewerUserName} — وضعیت: {review.Status}",
                    OccurredAt = review.RequestedAtUtc.ToLocalTime(),
                    Icon = "🔍"
                });

            if (review.StartedAtUtc.HasValue)
            {
                timeline.Add(
                    new SampleTimelineItemViewModel
                    {
                        EventType = "Review",
                        ReferenceCode = review.ReferenceCode,
                        Title = "بازبینی پژوهشی شروع شد",
                        Description = review.ReviewerUserName,
                        OccurredAt = review.StartedAtUtc.Value.ToLocalTime(),
                        Icon = "🔎"
                    });
            }

            if (review.ReviewedAtUtc.HasValue)
            {
                var reviewTitle =
                    review.Status switch
                    {
                        Domain.Entities.ResearchReview.ResearchReviewStatus.Approved
                            => "بازبینی پژوهشی تایید شد",

                        Domain.Entities.ResearchReview.ResearchReviewStatus.Rejected
                            => "بازبینی پژوهشی رد شد",

                        Domain.Entities.ResearchReview.ResearchReviewStatus.Cancelled
                            => "بازبینی پژوهشی لغو شد",

                        _
                            => "بازبینی پژوهشی بررسی شد"
                    };

                timeline.Add(
                    new SampleTimelineItemViewModel
                    {
                        EventType = "Review",
                        ReferenceCode = review.ReferenceCode,
                        Title = reviewTitle,
                        Description = review.ReviewNote,
                        OccurredAt = review.ReviewedAtUtc.Value.ToLocalTime(),
                        Icon =
                            review.Status ==
                            Domain.Entities.ResearchReview.ResearchReviewStatus.Approved
                                ? "✅"
                                : review.Status ==
                                  Domain.Entities.ResearchReview.ResearchReviewStatus.Rejected
                                    ? "❌"
                                    : "🔍"
                    });
            }
        }

        // ---------------------------------------------------------
        // Decisions
        // ---------------------------------------------------------

        var decisions =
            await _decisionLogService.GetByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        foreach (var decision in decisions)
        {
            timeline.Add(
                new SampleTimelineItemViewModel
                {
                    EventType = "Decision",
                    ReferenceCode = decision.ReferenceCode,
                    Title = decision.DecisionType,
                    Description = decision.Decision,
                    OccurredAt = decision.CreatedAt,
                    Icon = "🧠"
                });
        }

        /*
         * Results currently do not expose a timestamp in ResultDto.
         * Therefore they remain in the timeline but are placed after
         * dated events instead of inventing a date.
         */

        timeline =
            timeline
                .OrderByDescending(x => x.OccurredAt.HasValue)
                .ThenByDescending(x => x.OccurredAt)
                .ThenBy(x => x.ReferenceCode)
                .ToList();

        var model =
            new SampleDetailsViewModel
            {
                Id = sample.Id,

                ReferenceCode =
                    $"SMP-{sample.Id:D6}",

                SampleCode =
                    sample.SampleCode,

                Name =
                    sample.Name,

                SampleType =
                    sample.SampleType,

                Matrix =
                    sample.Matrix,

                PreparationMethod =
                    sample.PreparationMethod,

                CollectedAt =
                    sample.CollectedAt,

                ExternalReference =
                    sample.ExternalReference,

                Description =
                    sample.Description,

                Notes =
                    sample.Notes,

                ExperimentId =
                    experiment.Id,

                ExperimentTitle =
                    experiment.Title,

                WorkItemId =
                    workItem.Id,

                WorkItemReferenceCode =
                    $"CRH-{workItem.Id:D6}",

                WorkItemTitle =
                    workItem.Title,

                ProjectId =
                    project.Id,

                ProjectName =
                    project.Name,

                Timeline =
                    timeline
            };

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Researcher")]
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
    [Authorize(Roles = "Admin,Researcher")]
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