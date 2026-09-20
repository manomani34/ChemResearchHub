using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.DecisionLog;

public class DecisionLog : BaseEntity
{
    private DecisionLog()
    {
    }

    public DecisionLog(
        int workItemId,
        string decisionType,
        string decision,
        string? rationale,
        string? evidence,
        string? createdByUserId)
    {
        if (workItemId <= 0)
        {
            throw new ArgumentException(
                "WorkItemId must be greater than zero.",
                nameof(workItemId));
        }

        SetDecisionType(decisionType);
        SetDecision(decision);

        WorkItemId = workItemId;
        Rationale = rationale?.Trim();
        Evidence = evidence?.Trim();
        CreatedByUserId = createdByUserId?.Trim();
    }

    public int WorkItemId { get; private set; }

    public string DecisionType { get; private set; } = null!;

    public string Decision { get; private set; } = null!;

    public string? Rationale { get; private set; }

    public string? Evidence { get; private set; }

    public string? CreatedByUserId { get; private set; }

    private void SetDecisionType(
        string decisionType)
    {
        if (string.IsNullOrWhiteSpace(decisionType))
        {
            throw new ArgumentException(
                "Decision type is required.",
                nameof(decisionType));
        }

        DecisionType = decisionType.Trim();
    }

    private void SetDecision(
        string decision)
    {
        if (string.IsNullOrWhiteSpace(decision))
        {
            throw new ArgumentException(
                "Decision is required.",
                nameof(decision));
        }

        Decision = decision.Trim();
    }
}