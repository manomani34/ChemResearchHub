using ChemResearchHub.Application.Attachments.Dtos;

namespace ChemResearchHub.Application.Attachments.Interfaces;

public interface IAttachmentService
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
        string contentType,
        long fileSize,
        Stream content,
        string? description,
        string? uploadedByUserId,
        CancellationToken cancellationToken = default);

    Task<AttachmentDownloadDto?> GetDownloadAsync(
        int id,
        CancellationToken cancellationToken = default);
}