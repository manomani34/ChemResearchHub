namespace ChemResearchHub.Web.Models.Attachments;

public class AttachmentsListViewModel
{
    public IReadOnlyList<AttachmentListItemViewModel> Items { get; init; }
        = Array.Empty<AttachmentListItemViewModel>();
}

public class AttachmentListItemViewModel
{
    public int Id { get; init; }

    public int WorkItemId { get; init; }

    public string WorkItemTitle { get; init; } = string.Empty;

    public string OriginalFileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long FileSize { get; init; }

    public string? Description { get; init; }

    public string? UploadedByUserId { get; init; }

    public DateTime CreatedAt { get; init; }
}