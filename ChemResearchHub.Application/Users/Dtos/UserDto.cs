
namespace ChemResearchHub.Application.Users.Dtos;

public class UserDto
{
    public string Id { get; init; } = null!;

    public string FullName { get; init; } = string.Empty;

    public string? Email { get; init; }

    public bool IsActive { get; init; }

    public IReadOnlyList<string> RoleNames { get; init; }
        = Array.Empty<string>();
}
