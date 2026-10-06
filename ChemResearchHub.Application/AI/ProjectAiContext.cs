using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Application.Samples.Interfaces;

namespace ChemResearchHub.Application.AI;

public class ProjectAiContext
{
    private readonly IProjectService _projectService;
    private readonly IExperimentService _experimentService;
    private readonly ISampleService _sampleService;

    public ProjectAiContext(
        IProjectService projectService,
        IExperimentService experimentService,
        ISampleService sampleService)
    {
        _projectService = projectService;
        _experimentService = experimentService;
        _sampleService = sampleService;
    }

    public async Task<object?> GetSampleByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var sample =
            await _sampleService.GetByIdAsync(
                id,
                cancellationToken);

        if (sample is null)
            return null;

        return new
        {
            id = sample.Id,
            experimentId = sample.ExperimentId,
            sampleCode = sample.SampleCode,
            name = sample.Name,
            sampleType = sample.SampleType,
            matrix = sample.Matrix,
            preparationMethod = sample.PreparationMethod,
            collectedAt = sample.CollectedAt,
            externalReference = sample.ExternalReference,
            description = sample.Description,
            notes = sample.Notes
        };
    }

    public async Task<object?> GetSampleByCodeAsync(
        string sampleCode,
        CancellationToken cancellationToken = default)
    {
        var sample =
            await _sampleService.GetBySampleCodeAsync(
                sampleCode,
                cancellationToken);

        if (sample is null)
            return null;

        return new
        {
            id = sample.Id,
            experimentId = sample.ExperimentId,
            sampleCode = sample.SampleCode,
            name = sample.Name,
            sampleType = sample.SampleType,
            matrix = sample.Matrix,
            preparationMethod = sample.PreparationMethod,
            collectedAt = sample.CollectedAt,
            externalReference = sample.ExternalReference,
            description = sample.Description,
            notes = sample.Notes
        };
    }

    public async Task<object?> GetSamplesByExperimentTitleAsync(
        string experimentTitle,
        CancellationToken cancellationToken = default)
    {
        var experiment =
            await _experimentService.GetByTitleAsync(
                experimentTitle,
                cancellationToken);

        if (experiment is null)
            return null;

        var samples =
            await _sampleService.GetByExperimentIdAsync(
                experiment.Id,
                cancellationToken);

        return new
        {
            experimentId = experiment.Id,
            experimentTitle = experiment.Title,
            count = samples.Count,
            samples = samples.Select(x => new
            {
                id = x.Id,
                experimentId = x.ExperimentId,
                sampleCode = x.SampleCode,
                name = x.Name,
                sampleType = x.SampleType,
                matrix = x.Matrix,
                preparationMethod = x.PreparationMethod,
                collectedAt = x.CollectedAt,
                externalReference = x.ExternalReference,
                description = x.Description,
                notes = x.Notes
            })
        };
    }

    public async Task<object?> GetProjectExperimentSummaryAsync(
        string projectName,
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectService.GetByNameAsync(
                projectName,
                cancellationToken);

        if (project is null)
            return null;

        var statistics =
            await _experimentService.GetStatisticsByProjectIdAsync(
                project.Id,
                cancellationToken);

        return new
        {
            projectId = project.Id,
            projectName = project.Name,
            total = statistics.Total,
            completed = statistics.Completed,
            inProgress = statistics.InProgress,
            notStarted = statistics.NotStarted
        };
    }

    public async Task<object> GetExperimentStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var statistics =
            await _experimentService.GetStatisticsAsync(
                cancellationToken);

        return new
        {
            total = statistics.Total,
            completed = statistics.Completed,
            inProgress = statistics.InProgress,
            notStarted = statistics.NotStarted
        };
    }

    public async Task<object> SearchExperimentsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var experiments =
            await _experimentService.SearchAsync(
                searchTerm,
                cancellationToken);

        return new
        {
            searchTerm,
            count = experiments.Count,
            experiments = experiments.Select(x => new
            {
                id = x.Id,
                workItemId = x.WorkItemId,
                title = x.Title,
                description = x.Description,
                protocol = x.Protocol,
                startedAt = x.StartedAt,
                completedAt = x.CompletedAt,
                notes = x.Notes
            })
        };
    }

    public async Task<object?> GetExperimentByTitleAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        var experiment =
            await _experimentService.GetByTitleAsync(
                title,
                cancellationToken);

        if (experiment is null)
            return null;

        return new
        {
            id = experiment.Id,
            workItemId = experiment.WorkItemId,
            title = experiment.Title,
            description = experiment.Description,
            protocol = experiment.Protocol,
            startedAt = experiment.StartedAt,
            completedAt = experiment.CompletedAt,
            notes = experiment.Notes
        };
    }

    public async Task<object?> GetLatestExperimentAsync(
        CancellationToken cancellationToken = default)
    {
        var experiment =
            await _experimentService.GetLatestAsync(
                cancellationToken);

        if (experiment is null)
            return null;

        return new
        {
            id = experiment.Id,
            workItemId = experiment.WorkItemId,
            title = experiment.Title,
            description = experiment.Description,
            protocol = experiment.Protocol,
            startedAt = experiment.StartedAt,
            completedAt = experiment.CompletedAt,
            notes = experiment.Notes
        };
    }

    public async Task<object?> GetExperimentsByProjectNameAsync(
        string projectName,
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectService.GetByNameAsync(
                projectName,
                cancellationToken);

        if (project is null)
            return null;

        var experiments =
            await _experimentService.GetByProjectIdAsync(
                project.Id,
                cancellationToken);

        return new
        {
            projectId = project.Id,
            projectName = project.Name,
            count = experiments.Count,
            experiments = experiments.Select(x => new
            {
                id = x.Id,
                workItemId = x.WorkItemId,
                title = x.Title,
                description = x.Description,
                protocol = x.Protocol,
                startedAt = x.StartedAt,
                completedAt = x.CompletedAt,
                notes = x.Notes
            })
        };
    }

    public async Task<object?> GetProjectOverviewAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectService.GetByNameAsync(
                name,
                cancellationToken);

        if (project is null)
            return null;

        var today = DateTime.UtcNow.Date;

        int elapsedDays = 0;
        int remainingDays = 0;
        double progressPercentage = 0;

        if (project.StartDate.HasValue &&
            project.EndDate.HasValue)
        {
            var startDate = project.StartDate.Value.Date;
            var endDate = project.EndDate.Value.Date;

            var totalDays =
                (endDate - startDate).Days;

            elapsedDays =
                Math.Max(
                    0,
                    (today - startDate).Days);

            remainingDays =
                Math.Max(
                    0,
                    (endDate - today).Days);

            if (totalDays > 0)
            {
                progressPercentage =
                    Math.Min(
                        100,
                        Math.Max(
                            0,
                            elapsedDays * 100.0 / totalDays));
            }
        }

        return new
        {
            id = project.Id,
            name = project.Name,
            description = project.Description,
            startDate = project.StartDate,
            endDate = project.EndDate,
            createdAt = project.CreatedAt,
            isActive = project.IsActive,
            elapsedDays,
            remainingDays,
            progressPercentage =
                Math.Round(
                    progressPercentage,
                    1)
        };
    }

    public async Task<object> GetProjectStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var statistics =
            await _projectService.GetStatisticsAsync(
                cancellationToken);

        return new
        {
            total = statistics.Total,
            active = statistics.Active,
            inactive = statistics.Inactive
        };
    }

    public async Task<object> SearchProjectsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var projects =
            await _projectService.SearchAsync(
                searchTerm,
                cancellationToken);

        return new
        {
            searchTerm,
            count = projects.Count,
            projects = projects.Select(x => new
            {
                id = x.Id,
                name = x.Name,
                description = x.Description,
                startDate = x.StartDate,
                endDate = x.EndDate,
                isActive = x.IsActive
            })
        };
    }

    public async Task<object?> GetProjectByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectService.GetByNameAsync(
                name,
                cancellationToken);

        if (project is null)
            return null;

        return new
        {
            id = project.Id,
            name = project.Name,
            description = project.Description,
            startDate = project.StartDate,
            endDate = project.EndDate,
            createdAt = project.CreatedAt,
            isActive = project.IsActive
        };
    }

    public async Task<object?> GetLatestProjectAsync(
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectService.GetLatestAsync(
                cancellationToken);

        if (project is null)
            return null;

        return new
        {
            id = project.Id,
            name = project.Name,
            description = project.Description,
            startDate = project.StartDate,
            endDate = project.EndDate,
            createdAt = project.CreatedAt,
            isActive = project.IsActive
        };
    }

    public async Task<object> GetProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        var projects =
            await _projectService.GetAllAsync(
                cancellationToken);

        return new
        {
            count = projects.Count,
            projects = projects.Select(x => new
            {
                id = x.Id,
                name = x.Name,
                description = x.Description,
                startDate = x.StartDate,
                endDate = x.EndDate,
                isActive = x.IsActive
            })
        };
    }
}