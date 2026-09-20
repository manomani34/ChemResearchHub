namespace ChemResearchHub.Application.Attachments.Dtos;

public class AttachmentDto
{
    public int Id { get; init; }

    public int WorkItemId { get; init; }

    public string OriginalFileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long FileSize { get; init; }

    public string StoragePath { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? UploadedByUserId { get; init; }

    public DateTime CreatedAt { get; init; }
}