using ChemResearchHub.Application.ResearchReviews.Dtos;
using ChemResearchHub.Application.ResearchReviews.Interfaces;
using ChemResearchHub.Application.ResearchReviews.Repositories;

namespace ChemResearchHub.Application.ResearchReviews.Services;

public class ResearchReviewService : IResearchReviewService
{
    private readonly IResearchReviewRepository _repository;

    public ResearchReviewService(
        IResearchReviewRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<ResearchReviewDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
        => _repository.GetByWorkItemIdAsync(
            workItemId,
            cancellationToken);

    public Task<ResearchReviewDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(
            id,
            cancellationToken);

    public async Task<bool> RequestAsync(
        int workItemId,
        string requestedByUserId,
        string? reviewerUserId,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0 ||
            string.IsNullOrWhiteSpace(requestedByUserId))
        {
            return false;
        }

        if (await _repository.WorkItemExistsAsync(
                workItemId,
                cancellationToken) is false)
        {
            return false;
        }

        if (await _repository.HasOpenReviewAsync(
                workItemId,
                cancellationToken))
        {
            return false;
        }

        var entity = await _repository.AddAsync(
            workItemId,
            requestedByUserId,
            reviewerUserId,
            cancellationToken);

        if (entity is null)
            return false;

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> StartAsync(
        int reviewId,
        string reviewerUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetEntityByIdAsync(
            reviewId,
            cancellationToken);

        if (entity is null)
            return false;

        entity.StartReview(reviewerUserId);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> ApproveAsync(
        int reviewId,
        string reviewerUserId,
        string? note,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetEntityByIdAsync(
            reviewId,
            cancellationToken);

        if (entity is null)
            return false;

        entity.Approve(
            reviewerUserId,
            note,
            allowReviewerOverride: isAdmin);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> RejectAsync(
        int reviewId,
        string reviewerUserId,
        string note,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetEntityByIdAsync(
            reviewId,
            cancellationToken);

        if (entity is null)
            return false;

        entity.Reject(
            reviewerUserId,
            note,
            allowReviewerOverride: isAdmin);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> CancelAsync(
        int reviewId,
        string userId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetEntityByIdAsync(
            reviewId,
            cancellationToken);

        if (entity is null)
            return false;

        entity.Cancel(userId, note);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}