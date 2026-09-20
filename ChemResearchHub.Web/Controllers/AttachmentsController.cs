using System.Security.Claims;
using ChemResearchHub.Application.Attachments.Interfaces;
using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Models.Attachments;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

public class AttachmentsController : Controller
{
    private const long MaxFileSize =
        20 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".csv",
            ".xlsx",
            ".docx",
            ".txt",
            ".png",
            ".jpg",
            ".jpeg"
        };

    private readonly IAttachmentService _attachmentService;
    private readonly IWorkItemService _workItemService;
    private readonly IBoardService _boardService;

    public AttachmentsController(
        IAttachmentService attachmentService,
        IWorkItemService workItemService,
        IBoardService boardService)
    {
        _attachmentService = attachmentService;
        _workItemService = workItemService;
        _boardService = boardService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
    CancellationToken cancellationToken)
    {
        var attachments =
            await _attachmentService.GetAllAsync(
                cancellationToken);

        var workItems =
            await _workItemService.GetAllAsync(
                cancellationToken: cancellationToken);

        var workItemNames =
            workItems.ToDictionary(
                x => x.Id,
                x => x.Title);

        var items =
            attachments
                .Select(x => new AttachmentListItemViewModel
                {
                    Id = x.Id,

                    WorkItemId =
                        x.WorkItemId,

                    WorkItemTitle =
                        workItemNames.TryGetValue(
                            x.WorkItemId,
                            out var workItemTitle)
                            ? workItemTitle
                            : $"کار #{x.WorkItemId}",

                    OriginalFileName =
                        x.OriginalFileName,

                    ContentType =
                        x.ContentType,

                    FileSize =
                        x.FileSize,

                    Description =
                        x.Description,

                    UploadedByUserId =
                        x.UploadedByUserId,

                    CreatedAt =
                        x.CreatedAt
                })
                .ToList();

        return View(
            new AttachmentsListViewModel
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
        if (!await IsValidContextAsync(
                workItemId,
                projectId,
                boardId,
                cancellationToken))
        {
            return NotFound();
        }

        return View(
            new CreateAttachmentViewModel
            {
                WorkItemId = workItemId,
                ProjectId = projectId,
                BoardId = boardId
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateAttachmentViewModel model,
        CancellationToken cancellationToken)
    {
        if (!await IsValidContextAsync(
                model.WorkItemId,
                model.ProjectId,
                model.BoardId,
                cancellationToken))
        {
            return NotFound();
        }

        if (model.File is null ||
            model.File.Length <= 0)
        {
            ModelState.AddModelError(
                nameof(model.File),
                "Please select a valid file.");
        }
        else
        {
            var extension =
                Path.GetExtension(
                    model.File.FileName);

            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(model.File),
                    "This file type is not supported.");
            }

            if (model.File.Length > MaxFileSize)
            {
                ModelState.AddModelError(
                    nameof(model.File),
                    "Maximum file size is 20 MB.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            await using var stream =
                model.File!.OpenReadStream();

            var attachment =
                await _attachmentService.CreateAsync(
                    model.WorkItemId,
                    model.File.FileName,
                    model.File.ContentType,
                    model.File.Length,
                    stream,
                    model.Description,
                    userId,
                    cancellationToken);

            if (attachment is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The attachment could not be created.");

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
    public async Task<IActionResult> Download(
        int id,
        CancellationToken cancellationToken)
    {
        var file =
            await _attachmentService.GetDownloadAsync(
                id,
                cancellationToken);

        if (file is null)
        {
            return NotFound();
        }

        return File(
            file.Content,
            file.ContentType,
            file.FileName);
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