using ChemResearchHub.Application.Attachments.Dtos;

namespace ChemResearchHub.Application.Attachments.Repositories;

public interface IAttachmentRepository
{
    Task<IReadOnlyList<AttachmentDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttachmentDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default);

    Task<AttachmentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<AttachmentDto?> CreateAsync(
        int workItemId,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string? description,
        string? uploadedByUserId,
        CancellationToken cancellationToken = default);
}