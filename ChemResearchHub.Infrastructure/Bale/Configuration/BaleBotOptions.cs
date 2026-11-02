namespace ChemResearchHub.Infrastructure.Bale.Configuration;

public sealed class BaleBotOptions
{
    public const string SectionName = "BaleBot";

    public string Token { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = "https://tapi.bale.ai";

    public bool EnablePolling { get; set; } = true;

    public int PollingTimeoutSeconds { get; set; } = 30;
    public long GroupChatId { get; set; }
}
