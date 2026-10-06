namespace ChemResearchHub.Application.AI.Ollama;

public interface IOllamaClient
{
    Task<OllamaResponse> ChatAsync(
        List<OllamaMessage> messages,
        object[]? tools,
        CancellationToken cancellationToken = default);
}