using ChemResearchHub.Application.Attachments.Dtos;
using ChemResearchHub.Application.Attachments.Repositories;
using ChemResearchHub.Domain.Entities.Attachment;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly ApplicationDbContext _context;

    public AttachmentRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AttachmentDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _context.Attachments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new AttachmentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                OriginalFileName = x.OriginalFileName,
                ContentType = x.ContentType,
                FileSize = x.FileSize,
                Description = x.Description,
                UploadedByUserId = x.UploadedByUserId,
                CreatedAt = x.CreatedAt,
                StoragePath = x.StoragePath
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AttachmentDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Attachments
            .AsNoTracking()
            .Where(x => x.WorkItemId == workItemId)
            .OrderByDescending(x => x.Id)
            .Select(x => new AttachmentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                OriginalFileName = x.OriginalFileName,
                ContentType = x.ContentType,
                FileSize = x.FileSize,
                Description = x.Description,
                UploadedByUserId = x.UploadedByUserId,
                CreatedAt = x.CreatedAt,
                StoragePath = x.StoragePath,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AttachmentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Attachments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AttachmentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                OriginalFileName = x.OriginalFileName,
                ContentType = x.ContentType,
                FileSize = x.FileSize,
                Description = x.Description,
                UploadedByUserId = x.UploadedByUserId,
                CreatedAt = x.CreatedAt,
                StoragePath = x.StoragePath,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AttachmentDto?> CreateAsync(
        int workItemId,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string? description,
        string? uploadedByUserId,
        CancellationToken cancellationToken = default)
    {
        var attachment =
            new Attachment(
                workItemId,
                originalFileName,
                storedFileName,
                contentType,
                fileSize,
                storagePath,
                description,
                uploadedByUserId);

        await _context.Attachments.AddAsync(
            attachment,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        return new AttachmentDto
        {
            Id = attachment.Id,
            WorkItemId = attachment.WorkItemId,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            Description = attachment.Description,
            UploadedByUserId = attachment.UploadedByUserId,
            CreatedAt = attachment.CreatedAt,
            StoragePath = attachment.StoragePath,
        };
    }
}