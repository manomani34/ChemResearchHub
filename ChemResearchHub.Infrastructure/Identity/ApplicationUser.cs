using Microsoft.AspNetCore.Identity;

namespace ChemResearchHub.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Bale private chat ID linked to this application user.
    /// Null means the user has not linked a Bale account.
    /// </summary>
    public long? BaleChatId { get; set; }
}
