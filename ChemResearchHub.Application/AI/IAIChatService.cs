public interface IAIChatService
{
    Task<string> ChatAsync(string message, CancellationToken cancellationToken = default);
}