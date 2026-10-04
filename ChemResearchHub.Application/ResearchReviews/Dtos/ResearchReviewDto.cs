using ChemResearchHub.Domain.Entities.ResearchReview;

namespace ChemResearchHub.Application.ResearchReviews.Dtos;

public class ResearchReviewDto
{
    public int Id { get; init; }
    public int WorkItemId { get; init; }
    public string WorkItemTitle { get; init; } = string.Empty;
    public string RequestedByUserId { get; init; } = string.Empty;
    public string RequestedByUserName { get; init; } = string.Empty;
    public string? ReviewerUserId { get; init; }
    public string? ReviewerUserName { get; init; }
    public ResearchReviewStatus Status { get; init; }
    public DateTime RequestedAtUtc { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? ReviewedAtUtc { get; init; }
    public string? ReviewNote { get; init; }
    public string ReferenceCode =>
    $"REV-{Id:D6}";
}
