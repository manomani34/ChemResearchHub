using ChemResearchHub.Domain.Entities.DecisionLog;
using ChemResearchHub.Domain.Entities.WorkItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class DecisionLogConfiguration
    : IEntityTypeConfiguration<DecisionLog>
{
    public void Configure(
        EntityTypeBuilder<DecisionLog> builder)
    {
        builder.ToTable("DecisionLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WorkItemId)
            .IsRequired();

        builder.Property(x => x.DecisionType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Decision)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.Rationale)
            .HasMaxLength(10000)
            .IsRequired(false);

        builder.Property(x => x.Evidence)
            .HasMaxLength(10000)
            .IsRequired(false);

        builder.Property(x => x.CreatedByUserId)
            .HasMaxLength(450)
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

        builder.HasIndex(x => x.CreatedAt);
    }
}