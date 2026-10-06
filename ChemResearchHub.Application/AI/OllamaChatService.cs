using ChemResearchHub.Application.AI;
using ChemResearchHub.Application.AI.Experiment;
using ChemResearchHub.Application.AI.Ollama;
using ChemResearchHub.Application.AI.Prompting;
using ChemResearchHub.Application.AI.Sample;
using ChemResearchHub.Application.AI.Tools;

public class OllamaChatService : IAIChatService
{
    private readonly AiIntentRouter _intentRouter;
    private readonly SampleRequestHandler _sampleRequestHandler;
    private readonly ExperimentRequestHandler _experimentRequestHandler;
    private readonly IOllamaClient _ollamaClient;
    private readonly AiToolExecutor _toolExecutor;
    private readonly IAiPromptProvider _promptProvider;
    private readonly AiToolSelector _toolSelector;

    public OllamaChatService(
        IOllamaClient ollamaClient,
        AiIntentRouter intentRouter,
        SampleRequestHandler sampleRequestHandler,
        ExperimentRequestHandler experimentRequestHandler,
        AiToolExecutor toolExecutor,
        IAiPromptProvider promptProvider,
        AiToolSelector toolSelector)
    {
        _ollamaClient = ollamaClient;
        _intentRouter = intentRouter;
        _sampleRequestHandler = sampleRequestHandler;
        _experimentRequestHandler = experimentRequestHandler;
        _toolExecutor = toolExecutor;
        _promptProvider = promptProvider;
        _toolSelector = toolSelector;
    }

    public async Task<string> ChatAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var intent =
            _intentRouter.Detect(message);

        // =========================================================
        // DIRECT ROUTING
        // =========================================================

        if (intent == AiIntent.Sample)
        {
            return await _sampleRequestHandler.HandleAsync(
                message,
                cancellationToken);
        }

        if (intent == AiIntent.Experiment)
        {
            return await _experimentRequestHandler.HandleAsync(
                message,
                cancellationToken);
        }

        // =========================================================
        // NORMAL AI FLOW
        // =========================================================

        var messages = new List<OllamaMessage>
        {
            new()
            {
                Role = "system",
                Content = _promptProvider.GetSystemPrompt()
            },
            new()
            {
                Role = "user",
                Content = message
            }
        };

        var tools =
     _toolSelector.GetToolsForIntent(intent);

        var firstResponse =
            await _ollamaClient.ChatAsync(
                messages,
                tools,
                cancellationToken);

        if (firstResponse.Message?.ToolCalls is null ||
            firstResponse.Message.ToolCalls.Count == 0)
        {
            return
                firstResponse.Message?.Content?.Trim()
                ?? string.Empty;
        }

        messages.Add(
            firstResponse.Message);

        foreach (var toolCall in
         firstResponse.Message.ToolCalls)
        {
            var toolResult =
                await _toolExecutor.ExecuteAsync(
                    toolCall,
                    cancellationToken);

            if (toolResult is null)
                continue;

            messages.Add(
                new OllamaMessage
                {
                    Role = "tool",
                    Content =
                        """
                AUTHORITATIVE CHEMRESEARCHHUB DATA.

                Use ONLY the data below to answer the user's question.
                Do not invent, modify, replace, or add any database value.
                Do not create additional records.

                TOOL RESULT:
                """
                        + Environment.NewLine
                        + toolResult
                });
        }

        var finalResponse =
            await _ollamaClient.ChatAsync(
                messages,
                tools: null,
                cancellationToken);

        return
            finalResponse.Message?.Content?.Trim()
            ?? string.Empty;
    }
    
}