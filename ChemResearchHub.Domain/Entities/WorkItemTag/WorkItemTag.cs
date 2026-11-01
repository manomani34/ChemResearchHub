namespace ChemResearchHub.Domain.Entities.WorkItemTag;

public class WorkItemTag
{
    private WorkItemTag()
    {
    }

    public WorkItemTag(
        int workItemId,
        int tagId)
    {
        if (workItemId <= 0)
        {
            throw new ArgumentException(
                "WorkItemId must be greater than zero.",
                nameof(workItemId));
        }

        if (tagId <= 0)
        {
            throw new ArgumentException(
                "TagId must be greater than zero.",
                nameof(tagId));
        }

        WorkItemId = workItemId;
        TagId = tagId;
    }

    public int WorkItemId { get; private set; }

    public int TagId { get; private set; }
}
