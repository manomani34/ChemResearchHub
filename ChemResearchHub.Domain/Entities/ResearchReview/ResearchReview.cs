using ChemResearchHub.Domain.Common;
using WorkItemEntity = ChemResearchHub.Domain.Entities.WorkItem.WorkItem;

namespace ChemResearchHub.Domain.Entities.ResearchReview;

public class ResearchReview : BaseEntity
{
    private ResearchReview() { }

    public ResearchReview(
        WorkItemEntity workItem,
        string requestedByUserId,
        string? reviewerUserId)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        if (string.IsNullOrWhiteSpace(requestedByUserId))
            throw new ArgumentException("Requested by user is required.", nameof(requestedByUserId));

        WorkItemId = workItem.Id;
        WorkItem = workItem;
        RequestedByUserId = requestedByUserId.Trim();
        ReviewerUserId = string.IsNullOrWhiteSpace(reviewerUserId) ? null : reviewerUserId.Trim();
        Status = ResearchReviewStatus.Pending;
        RequestedAtUtc = DateTime.UtcNow;
    }

    public int WorkItemId { get; private set; }
    public WorkItemEntity WorkItem { get; private set; } = null!;

    public string RequestedByUserId { get; private set; } = string.Empty;
    public string? ReviewerUserId { get; private set; }

    public ResearchReviewStatus Status { get; private set; }
    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public string? ReviewNote { get; private set; }

    public void StartReview(string reviewerUserId)
    {
        if (string.IsNullOrWhiteSpace(reviewerUserId))
            throw new ArgumentException("Reviewer user is required.", nameof(reviewerUserId));

        if (Status is ResearchReviewStatus.Approved
            or ResearchReviewStatus.Rejected
            or ResearchReviewStatus.Cancelled)
            throw new InvalidOperationException("A closed review cannot be started.");

        ReviewerUserId = reviewerUserId.Trim();
        Status = ResearchReviewStatus.InReview;
        StartedAtUtc ??= DateTime.UtcNow;
    }

    public void Approve(
    string reviewerUserId,
    string? note,
    bool allowReviewerOverride = false)
    {
        EnsureReviewer(reviewerUserId, allowReviewerOverride);
        EnsureOpen();

        Status = ResearchReviewStatus.Approved;
        ReviewedAtUtc = DateTime.UtcNow;
        ReviewNote = NormalizeNote(note);
    }

    public void Reject(
    string reviewerUserId,
    string note,
    bool allowReviewerOverride = false)
    {
        EnsureReviewer(reviewerUserId, allowReviewerOverride);
        EnsureOpen();

        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException(
                "A rejection note is required.",
                nameof(note));

        Status = ResearchReviewStatus.Rejected;
        ReviewedAtUtc = DateTime.UtcNow;
        ReviewNote = NormalizeNote(note);
    }

    public void Cancel(string userId, string? note)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User is required.", nameof(userId));

        if (Status is ResearchReviewStatus.Approved
            or ResearchReviewStatus.Rejected
            or ResearchReviewStatus.Cancelled)
            return;

        Status = ResearchReviewStatus.Cancelled;
        ReviewedAtUtc = DateTime.UtcNow;
        ReviewNote = NormalizeNote(note);
    }

    private void EnsureReviewer(
    string reviewerUserId,
    bool allowReviewerOverride = false)
    {
        if (string.IsNullOrWhiteSpace(reviewerUserId))
            throw new ArgumentException(
                "Reviewer user is required.",
                nameof(reviewerUserId));

        if (!allowReviewerOverride
            && !string.IsNullOrWhiteSpace(ReviewerUserId)
            && !string.Equals(
                ReviewerUserId,
                reviewerUserId.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "This review is assigned to another reviewer.");
        }

        ReviewerUserId = reviewerUserId.Trim();
        StartedAtUtc ??= DateTime.UtcNow;
    }

    private void EnsureOpen()
    {
        if (Status is ResearchReviewStatus.Approved
            or ResearchReviewStatus.Rejected
            or ResearchReviewStatus.Cancelled)
            throw new InvalidOperationException("This review is already closed.");

        Status = ResearchReviewStatus.InReview;
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;

        var value = note.Trim();
        if (value.Length > 2000)
            throw new ArgumentException("Review note cannot exceed 2000 characters.", nameof(note));

        return value;
    }
}
