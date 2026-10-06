namespace ChemResearchHub.Application.Experiments.Dtos;

public class ExperimentStatistics
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int InProgress { get; set; }
    public int NotStarted { get; set; }
}