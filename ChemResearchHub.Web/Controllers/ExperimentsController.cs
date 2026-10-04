using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.DecisionLogs.Interfaces;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.ResearchReviews.Interfaces;
using ChemResearchHub.Application.Results.Interfaces;
using ChemResearchHub.Application.Samples.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Models.Experiments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

[Authorize]
public class ExperimentsController : Controller
{
    private readonly IExperimentService _experimentService;
    private readonly IWorkItemService _workItemService;
    private readonly IBoardService _boardService;
    private readonly ISampleService _sampleService;
    private readonly IResultService _resultService;
    private readonly IResearchReviewService _researchReviewService;
    private readonly IDecisionLogService _decisionLogService;

    public ExperimentsController(
        IExperimentService experimentService,
        IWorkItemService workItemService,
        IBoardService boardService,
        ISampleService sampleService,
        IResultService resultService,
        IResearchReviewService researchReviewService,
        IDecisionLogService decisionLogService)
    {
        _experimentService = experimentService;
        _workItemService = workItemService;
        _boardService = boardService;
        _sampleService = sampleService;
        _resultService = resultService;
        _researchReviewService = researchReviewService;
        _decisionLogService = decisionLogService;
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
                    ReferenceCode = x.ReferenceCode,
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
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var experiment =
            await _experimentService.GetByIdAsync(
                id,
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

        /*
         * =====================================================
         * Samples + Results
         * =====================================================
         */

        var samples =
            await _sampleService.GetByExperimentIdAsync(
                experiment.Id,
                cancellationToken);

        var sampleViewModels =
            new List<ExperimentSampleViewModel>();

        foreach (var sample in samples)
        {
            var results =
                await _resultService.GetBySampleIdAsync(
                    sample.Id,
                    cancellationToken);

            sampleViewModels.Add(
                new ExperimentSampleViewModel
                {
                    Id = sample.Id,
                    ReferenceCode = $"SMP-{sample.Id:D6}",
                    SampleCode = sample.SampleCode,
                    Name = sample.Name,
                    SampleType = sample.SampleType,
                    Matrix = sample.Matrix,
                    CollectedAt = sample.CollectedAt,

                    Results = results
                        .Select(result => new ExperimentResultViewModel
                        {
                            Id = result.Id,
                            ReferenceCode = result.ReferenceCode,
                            MetricName = result.MetricName,
                            NumericValue = result.NumericValue,
                            TextValue = result.TextValue,
                            Unit = result.Unit,
                            Method = result.Method,
                            Status = result.Status,
                            Evidence = result.Evidence,
                            Notes = result.Notes
                        })
                        .ToList()
                });
        }


        /*
         * =====================================================
         * Research Reviews
         * =====================================================
         */

        var reviews =
            await _researchReviewService.GetByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        var reviewViewModels =
            reviews
                .Select(review => new ExperimentReviewViewModel
                {
                    Id = review.Id,
                    ReferenceCode = review.ReferenceCode,
                    RequestedByUserName = review.RequestedByUserName,
                    ReviewerUserName = review.ReviewerUserName,
                    Status = review.Status.ToString(),
                    RequestedAtUtc = review.RequestedAtUtc,
                    StartedAtUtc = review.StartedAtUtc,
                    ReviewedAtUtc = review.ReviewedAtUtc,
                    ReviewNote = review.ReviewNote
                })
                .OrderByDescending(x => x.RequestedAtUtc)
                .ToList();


        /*
         * =====================================================
         * Decisions
         * =====================================================
         */

        var decisions =
            await _decisionLogService.GetByWorkItemIdAsync(
                workItem.Id,
                cancellationToken);

        var decisionViewModels =
            decisions
                .Select(decision => new ExperimentDecisionViewModel
                {
                    Id = decision.Id,
                    ReferenceCode = decision.ReferenceCode,
                    DecisionType = decision.DecisionType,
                    Decision = decision.Decision,
                    Rationale = decision.Rationale,
                    Evidence = decision.Evidence,
                    CreatedAt = decision.CreatedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToList();


        /*
         * =====================================================
         * Experiment Timeline
         * =====================================================
         */

        var timeline =
            new List<ExperimentTimelineItemViewModel>();


        // Experiment started

        if (experiment.StartedAt.HasValue)
        {
            timeline.Add(
                new ExperimentTimelineItemViewModel
                {
                    EventType = "Experiment",
                    ReferenceCode = experiment.ReferenceCode,
                    Title = "آزمایش شروع شد",
                    Description = experiment.Title,
                    OccurredAt = experiment.StartedAt,
                    Icon = "🔬"
                });
        }


        // Samples

        foreach (var sample in samples)
        {
            if (sample.CollectedAt.HasValue)
            {
                timeline.Add(
                    new ExperimentTimelineItemViewModel
                    {
                        EventType = "Sample",
                        ReferenceCode = $"SMP-{sample.Id:D6}",
                        Title = "نمونه ثبت شد",
                        Description =
                            string.IsNullOrWhiteSpace(sample.Name)
                                ? sample.SampleCode
                                : sample.Name,
                        OccurredAt = sample.CollectedAt,
                        Icon = "🧫"
                    });
            }
        }


        // Results

        foreach (var sample in sampleViewModels)
        {
            foreach (var result in sample.Results)
            {
                /*
                 * ResultDto فعلاً Timestamp ندارد.
                 * بنابراین تاریخ جعلی تولید نمی‌کنیم.
                 */

                timeline.Add(
                    new ExperimentTimelineItemViewModel
                    {
                        EventType = "Result",
                        ReferenceCode = result.ReferenceCode,
                        Title = $"نتیجه ثبت شد: {result.MetricName}",
                        Description =
                            result.NumericValue.HasValue
                                ? $"{result.NumericValue} {result.Unit}".Trim()
                                : result.TextValue,
                        OccurredAt = null,
                        Icon = "📊"
                    });
            }
        }


        // Reviews

        foreach (var review in reviews)
        {
            timeline.Add(
                new ExperimentTimelineItemViewModel
                {
                    EventType = "Review",
                    ReferenceCode = review.ReferenceCode,
                    Title = "درخواست Review ثبت شد",
                    Description =
                        $"درخواست‌کننده: {review.RequestedByUserName}",
                    OccurredAt = review.RequestedAtUtc,
                    Icon = "🔍"
                });

            if (review.StartedAtUtc.HasValue)
            {
                timeline.Add(
                    new ExperimentTimelineItemViewModel
                    {
                        EventType = "Review",
                        ReferenceCode = review.ReferenceCode,
                        Title = "Review شروع شد",
                        Description = review.ReviewerUserName,
                        OccurredAt = review.StartedAtUtc,
                        Icon = "🔎"
                    });
            }

            if (review.ReviewedAtUtc.HasValue)
            {
                var reviewTitle =
                    review.Status switch
                    {
                        Domain.Entities.ResearchReview.ResearchReviewStatus.Approved
                            => "Review تایید شد",

                        Domain.Entities.ResearchReview.ResearchReviewStatus.Rejected
                            => "Review رد شد",

                        Domain.Entities.ResearchReview.ResearchReviewStatus.Cancelled
                            => "Review لغو شد",

                        _
                            => "Review بررسی شد"
                    };

                timeline.Add(
                    new ExperimentTimelineItemViewModel
                    {
                        EventType = "Review",
                        ReferenceCode = review.ReferenceCode,
                        Title = reviewTitle,
                        Description = review.ReviewNote,
                        OccurredAt = review.ReviewedAtUtc,
                        Icon = review.Status switch
                        {
                            Domain.Entities.ResearchReview.ResearchReviewStatus.Approved
                                => "✅",

                            Domain.Entities.ResearchReview.ResearchReviewStatus.Rejected
                                => "❌",

                            Domain.Entities.ResearchReview.ResearchReviewStatus.Cancelled
                                => "🚫",

                            _
                                => "🔍"
                        }
                    });
            }
        }


        // Decisions

        foreach (var decision in decisions)
        {
            timeline.Add(
                new ExperimentTimelineItemViewModel
                {
                    EventType = "Decision",
                    ReferenceCode = decision.ReferenceCode,
                    Title = decision.DecisionType,
                    Description = decision.Decision,
                    OccurredAt = decision.CreatedAt,
                    Icon = "🧠"
                });
        }


        // Experiment completed

        if (experiment.CompletedAt.HasValue)
        {
            timeline.Add(
                new ExperimentTimelineItemViewModel
                {
                    EventType = "Experiment",
                    ReferenceCode = experiment.ReferenceCode,
                    Title = "آزمایش تکمیل شد",
                    Description = experiment.Title,
                    OccurredAt = experiment.CompletedAt,
                    Icon = "✅"
                });
        }


        timeline =
            timeline
                .OrderByDescending(x => x.OccurredAt.HasValue)
                .ThenByDescending(x => x.OccurredAt)
                .ThenBy(x => x.ReferenceCode)
                .ToList();


        var model = new ExperimentDetailsViewModel
        {
            Id = experiment.Id,
            ReferenceCode = experiment.ReferenceCode,

            WorkItemId = workItem.Id,
            WorkItemReferenceCode = $"CRH-{workItem.Id:D6}",
            WorkItemTitle = workItem.Title,

            Title = experiment.Title,
            Description = experiment.Description,
            Protocol = experiment.Protocol,
            StartedAt = experiment.StartedAt,
            CompletedAt = experiment.CompletedAt,
            Notes = experiment.Notes,

            Samples = sampleViewModels,
            Reviews = reviewViewModels,
            Decisions = decisionViewModels,
            Timeline = timeline
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
    [Authorize(Roles = "Admin,Researcher")]
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