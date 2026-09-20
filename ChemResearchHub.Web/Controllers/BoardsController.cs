using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Models.Boards;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

public class BoardsController : Controller
{
    private readonly IBoardService _boardService;
    private readonly IProjectService _projectService;
    private readonly IWorkItemService _workItemService;

    public BoardsController(
        IBoardService boardService,
        IProjectService projectService,
        IWorkItemService workItemService)
    {
        _boardService = boardService;
        _projectService = projectService;
        _workItemService = workItemService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
    CancellationToken cancellationToken)
    {
        var projects =
            await _projectService.GetAllAsync(
                cancellationToken);

        var projectNames =
            projects.ToDictionary(
                x => x.Id,
                x => x.Name);

        var items =
            new List<BoardListItemViewModel>();

        foreach (var project in projects)
        {
            var boards =
                await _boardService.GetByProjectIdAsync(
                    project.Id,
                    cancellationToken);

            foreach (var board in boards)
            {
                items.Add(
                    new BoardListItemViewModel
                    {
                        Id = board.Id,
                        ProjectId = board.ProjectId,
                        ProjectName =
                            projectNames.TryGetValue(
                                board.ProjectId,
                                out var projectName)
                                ? projectName
                                : $"پروژه #{board.ProjectId}",

                        Name = board.Name,

                        ColumnCount =
                            board.Columns?.Count ?? 0
                    });
            }
        }

        var model =
            new BoardsListViewModel
            {
                Items = items
            };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int projectId,
        int? boardId,
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

        var boards =
            await _boardService.GetByProjectIdAsync(
                projectId,
                cancellationToken);

        if (boards.Count == 0)
        {
            ViewBag.Project = project;

            return View(
                new KanbanViewModel
                {
                    Board = null!
                });
        }

        var selectedBoard =
            boardId.HasValue
                ? boards.FirstOrDefault(
                    x => x.Id == boardId.Value)
                : boards.First();

        if (selectedBoard is null)
        {
            return NotFound();
        }

        var workItemsByColumn =
            new Dictionary<
                int,
                IReadOnlyList<
                    ChemResearchHub.Application.WorkItems.Dtos.WorkItemDto>>();

        foreach (var column in selectedBoard.Columns)
        {
            var workItems =
                await _workItemService.GetByBoardColumnIdAsync(
                    column.Id,
                    cancellationToken);

            workItemsByColumn[column.Id] = workItems;
        }

        ViewBag.Project = project;
        ViewBag.Boards = boards;

        return View(
            new KanbanViewModel
            {
                Board = selectedBoard,
                WorkItemsByColumn = workItemsByColumn
            });
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        int projectId,
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

        ViewBag.Project = project;

        return View(
            new CreateBoardViewModel
            {
                ProjectId = projectId
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateBoardViewModel model,
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

        if (!ModelState.IsValid)
        {
            ViewBag.Project = project;

            return View(model);
        }

        await _boardService.CreateAsync(
            model.ProjectId,
            model.Name,
            cancellationToken);

        return RedirectToAction(
            nameof(Index),
            new
            {
                projectId = model.ProjectId
            });
    }
}