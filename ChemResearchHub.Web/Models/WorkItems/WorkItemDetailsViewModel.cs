using ChemResearchHub.Application.Attachments.Dtos;
using ChemResearchHub.Application.DecisionLogs.Dtos;
using ChemResearchHub.Application.Experiments.Dtos;
using ChemResearchHub.Application.Results.Dtos;
using ChemResearchHub.Application.Samples.Dtos;
using ChemResearchHub.Domain.Enums;


namespace ChemResearchHub.Web.Models.WorkItems;

public class WorkItemDetailsViewModel
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int BoardId { get; set; }

    public int BoardColumnId { get; set; }

    public string ColumnName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public WorkItemType Type { get; set; }

    public int Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsCompleted { get; set; }

    public int SortOrder { get; set; }

    public string? AssignedToUserId { get; set; }

    public string? AssignedToUserName { get; set; }

    public IReadOnlyList<ExperimentDto> Experiments { get; set; }
        = Array.Empty<ExperimentDto>();

    public IReadOnlyDictionary<int, IReadOnlyList<SampleDto>> SamplesByExperimentId
    {
        get;
        set;
    } = new Dictionary<int, IReadOnlyList<SampleDto>>();

    public IReadOnlyDictionary<int, IReadOnlyList<ResultDto>> ResultsBySampleId
    {
        get;
        set;
    } = new Dictionary<int, IReadOnlyList<ResultDto>>();

    public IReadOnlyList<AttachmentDto> Attachments { get; set; }
    = Array.Empty<AttachmentDto>();

    public IReadOnlyList<DecisionLogDto> DecisionLogs { get; set; }
    = Array.Empty<DecisionLogDto>();
}