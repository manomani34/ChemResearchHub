using ChemResearchHub.Application.Attachments.Dtos;
using ChemResearchHub.Application.Attachments.Interfaces;
using ChemResearchHub.Application.Attachments.Repositories;
using ChemResearchHub.Application.Attachments.Storage;

namespace ChemResearchHub.Application.Attachments.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentStorage _attachmentStorage;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IAttachmentStorage attachmentStorage)
    {
        _attachmentRepository = attachmentRepository;
        _attachmentStorage = attachmentStorage;
    }

    public async Task<IReadOnlyList<AttachmentDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _attachmentRepository.GetAllAsync(
            cancellationToken);
    }
    public async Task<IReadOnlyList<AttachmentDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
        {
            return Array.Empty<AttachmentDto>();
        }

        return await _attachmentRepository.GetByWorkItemIdAsync(
            workItemId,
            cancellationToken);
    }

    public async Task<AttachmentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _attachmentRepository.GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<AttachmentDto?> CreateAsync(
        int workItemId,
        string originalFileName,
        string contentType,
        long fileSize,
        Stream content,
        string? description,
        string? uploadedByUserId,
        CancellationToken cancellationToken = default)
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

        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (fileSize <= 0)
        {
            throw new ArgumentException(
                "File size must be greater than zero.",
                nameof(fileSize));
        }

        var safeOriginalFileName =
            Path.GetFileName(
                originalFileName.Trim());

        var extension =
            Path.GetExtension(
                safeOriginalFileName)
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new InvalidOperationException(
                "The uploaded file must have an extension.");
        }

        var storagePath =
            await _attachmentStorage.SaveAsync(
                content,
                extension,
                cancellationToken);

        try
        {
            return await _attachmentRepository.CreateAsync(
                workItemId,
                safeOriginalFileName,
                Path.GetFileName(storagePath),
                contentType,
                fileSize,
                storagePath,
                description,
                uploadedByUserId,
                cancellationToken);
        }
        catch
        {
            await _attachmentStorage.DeleteAsync(
                storagePath,
                cancellationToken);

            throw;
        }
    }

    public async Task<AttachmentDownloadDto?> GetDownloadAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var attachment =
            await _attachmentRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (attachment is null)
        {
            return null;
        }

        var content =
            await _attachmentStorage.OpenReadAsync(
                attachment.StoragePath,
                cancellationToken);

        if (content is null)
        {
            return null;
        }

        return new AttachmentDownloadDto
        {
            FileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            Content = content
        };
    }
}