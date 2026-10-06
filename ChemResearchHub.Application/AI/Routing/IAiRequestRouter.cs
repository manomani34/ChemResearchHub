using ChemResearchHub.Application.AI;

namespace ChemResearchHub.Application.AI.Routing;

public interface IAiRequestRouter
{
    AiIntent Detect(string message);
}