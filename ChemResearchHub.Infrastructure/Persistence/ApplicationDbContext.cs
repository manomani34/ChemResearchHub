using ChemResearchHub.Domain.Entities.Attachment;
using ChemResearchHub.Domain.Entities.Board;
using ChemResearchHub.Domain.Entities.DecisionLog;
using ChemResearchHub.Domain.Entities.Experiment;
using ChemResearchHub.Domain.Entities.Project;
using ChemResearchHub.Domain.Entities.Result;
using ChemResearchHub.Domain.Entities.Sample;
using ChemResearchHub.Domain.Entities.WorkItem;
using ChemResearchHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DecisionLog> DecisionLogs => Set<DecisionLog>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<Sample> Samples => Set<Sample>();
    public DbSet<Experiment> Experiments => Set<Experiment>();
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    public DbSet<Board> Boards => Set<Board>();

    public DbSet<BoardColumn> BoardColumns => Set<BoardColumn>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}