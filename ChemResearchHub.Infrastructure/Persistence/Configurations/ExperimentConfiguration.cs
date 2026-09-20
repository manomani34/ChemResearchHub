using ChemResearchHub.Domain.Entities.Experiment;
using ChemResearchHub.Domain.Entities.WorkItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class ExperimentConfiguration
    : IEntityTypeConfiguration<Experiment>
{
    public void Configure(
        EntityTypeBuilder<Experiment> builder)
    {
        builder.ToTable("Experiments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WorkItemId)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Description)
            .HasMaxLength(5000)
            .IsRequired(false);

        builder.Property(x => x.Protocol)
            .HasMaxLength(10000)
            .IsRequired(false);

        builder.Property(x => x.StartedAt)
            .IsRequired(false);

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        builder.Property(x => x.Notes)
            .HasMaxLength(10000)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .IsRequired(false);

        builder.HasOne<WorkItem>()
            .WithMany()
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.WorkItemId);
    }
}