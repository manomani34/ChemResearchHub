using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.Project;

public class Project : BaseEntity
{
    private Project()
    {
    }

    public Project(
        string name,
        string? description = null)
    {
        SetName(name);
        Description = description;
    }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public void Update(
        string name,
        string? description)
    {
        SetName(name);

        Description = description;

        ModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Project name is required.",
                nameof(name));
        }

        Name = name.Trim();
    }
}