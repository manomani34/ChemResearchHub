using ChemResearchHub.Application.ResearchReviews.Dtos;
using ChemResearchHub.Application.ResearchReviews.Repositories;
using ChemResearchHub.Domain.Entities.ResearchReview;
using ChemResearchHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class ResearchReviewRepository : IResearchReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ResearchReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ResearchReviewDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from review in _context.Set<ResearchReview>().AsNoTracking()
            join workItem in _context.WorkItems.AsNoTracking()
                on review.WorkItemId equals workItem.Id
            join requestedBy in _context.Users.AsNoTracking()
                on review.RequestedByUserId equals requestedBy.Id
            join reviewer in _context.Users.AsNoTracking()
                on review.ReviewerUserId equals reviewer.Id into reviewerGroup
            from reviewer in reviewerGroup.DefaultIfEmpty()
            where review.WorkItemId == workItemId
            orderby review.RequestedAtUtc descending, review.Id descending
            select new ResearchReviewDto
            {
                Id = review.Id,
                WorkItemId = review.WorkItemId,
                WorkItemTitle = workItem.Title,
                RequestedByUserId = review.RequestedByUserId,
                RequestedByUserName = requestedBy.FullName,
                ReviewerUserId = review.ReviewerUserId,
                ReviewerUserName = reviewer == null ? null : reviewer.FullName,
                Status = review.Status,
                RequestedAtUtc = review.RequestedAtUtc,
                StartedAtUtc = review.StartedAtUtc,
                ReviewedAtUtc = review.ReviewedAtUtc,
                ReviewNote = review.ReviewNote
            }).ToListAsync(cancellationToken);
    }

    public async Task<ResearchReviewDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await (
            from review in _context.Set<ResearchReview>().AsNoTracking()
            join workItem in _context.WorkItems.AsNoTracking()
                on review.WorkItemId equals workItem.Id
            join requestedBy in _context.Users.AsNoTracking()
                on review.RequestedByUserId equals requestedBy.Id
            join reviewer in _context.Users.AsNoTracking()
                on review.ReviewerUserId equals reviewer.Id into reviewerGroup
            from reviewer in reviewerGroup.DefaultIfEmpty()
            where review.Id == id
            select new ResearchReviewDto
            {
                Id = review.Id,
                WorkItemId = review.WorkItemId,
                WorkItemTitle = workItem.Title,
                RequestedByUserId = review.RequestedByUserId,
                RequestedByUserName = requestedBy.FullName,
                ReviewerUserId = review.ReviewerUserId,
                ReviewerUserName = reviewer == null ? null : reviewer.FullName,
                Status = review.Status,
                RequestedAtUtc = review.RequestedAtUtc,
                StartedAtUtc = review.StartedAtUtc,
                ReviewedAtUtc = review.ReviewedAtUtc,
                ReviewNote = review.ReviewNote
            }).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ResearchReview?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.Set<ResearchReview>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> HasOpenReviewAsync(int workItemId, CancellationToken cancellationToken = default)
        => _context.Set<ResearchReview>()
            .AnyAsync(
                x => x.WorkItemId == workItemId
                    && (x.Status == ResearchReviewStatus.Pending || x.Status == ResearchReviewStatus.InReview),
                cancellationToken);

    public Task<bool> WorkItemExistsAsync(int workItemId, CancellationToken cancellationToken = default)
        => _context.WorkItems.AnyAsync(x => x.Id == workItemId, cancellationToken);

    public async Task<ResearchReview?> AddAsync(
        int workItemId,
        string requestedByUserId,
        string? reviewerUserId,
        CancellationToken cancellationToken = default)
    {
        var workItem = await _context.WorkItems
            .FirstOrDefaultAsync(x => x.Id == workItemId, cancellationToken);

        if (workItem is null)
            return null;

        var entity = new ResearchReview(
            workItem,
            requestedByUserId,
            reviewerUserId);

        await _context.Set<ResearchReview>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
