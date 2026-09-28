using ChemResearchHub.Application.ResearchReviews.Dtos;
using ChemResearchHub.Domain.Entities.ResearchReview;

namespace ChemResearchHub.Application.ResearchReviews.Repositories;

public interface IResearchReviewRepository
{
    Task<IReadOnlyList<ResearchReviewDto>> GetByWorkItemIdAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<ResearchReviewDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ResearchReview?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasOpenReviewAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<bool> WorkItemExistsAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<ResearchReview?> AddAsync(int workItemId, string requestedByUserId, string? reviewerUserId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
