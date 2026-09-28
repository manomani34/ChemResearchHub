using ChemResearchHub.Application.ResearchReviews.Dtos;

namespace ChemResearchHub.Application.ResearchReviews.Interfaces;

public interface IResearchReviewService
{
    Task<IReadOnlyList<ResearchReviewDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default);

    Task<ResearchReviewDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> RequestAsync(
        int workItemId,
        string requestedByUserId,
        string? reviewerUserId,
        CancellationToken cancellationToken = default);

    Task<bool> StartAsync(
        int reviewId,
        string reviewerUserId,
        CancellationToken cancellationToken = default);

    Task<bool> ApproveAsync(
        int reviewId,
        string reviewerUserId,
        string? note,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<bool> RejectAsync(
        int reviewId,
        string reviewerUserId,
        string note,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<bool> CancelAsync(
        int reviewId,
        string userId,
        string? note,
        CancellationToken cancellationToken = default);
}