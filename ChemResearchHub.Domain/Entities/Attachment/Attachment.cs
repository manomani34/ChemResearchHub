using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.Attachment;

public class Attachment : BaseEntity
{
    private Attachment()
    {
    }

    public Attachment(
        int workItemId,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string? description,
        string? uploadedByUserId)
    {
        if (workItemId <= 0)
        {
            throw new ArgumentException(
                "WorkItemId must be greater than zero.",
                nameof(workItemId));
        }

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ArgumentException(
                "Original file name is required.",
                nameof(originalFileName));
        }

        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            throw new ArgumentException(
                "Stored file name is required.",
                nameof(storedFileName));
        }

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new ArgumentException(
                "Storage path is required.",
                nameof(storagePath));
        }

        WorkItemId = workItemId;
        OriginalFileName = originalFileName.Trim();
        StoredFileName = storedFileName.Trim();
        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType.Trim();
        FileSize = fileSize;
        StoragePath = storagePath.Trim();
        Description = description?.Trim();
        UploadedByUserId = uploadedByUserId?.Trim();
    }

    public int WorkItemId { get; private set; }

    public string OriginalFileName { get; private set; } = null!;

    public string StoredFileName { get; private set; } = null!;

    public string ContentType { get; private set; } = null!;

    public long FileSize { get; private set; }

    public string StoragePath { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? UploadedByUserId { get; private set; }

    public void UpdateDescription(
        string? description)
    {
        Description = description?.Trim();

        ModifiedAt = DateTime.UtcNow;
    }
}