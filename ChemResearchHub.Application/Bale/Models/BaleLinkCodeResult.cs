namespace ChemResearchHub.Application.Bale.Models;

public sealed class BaleLinkCodeResult
{
    public bool Succeeded { get; init; }

    public string? Code { get; init; }

    public DateTime? ExpiresAtUtc { get; init; }

    public string? Error { get; init; }

    public static BaleLinkCodeResult Success(
        string code,
        DateTime expiresAtUtc)
    {
        return new BaleLinkCodeResult
        {
            Succeeded = true,
            Code = code,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public static BaleLinkCodeResult Failure(string error)
    {
        return new BaleLinkCodeResult
        {
            Succeeded = false,
            Error = error
        };
    }
}