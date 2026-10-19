using ChemResearchHub.Domain.Entities.WorkItemAssignment;
using ChemResearchHub.Domain.Entities.WorkItem;
using ChemResearchHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class WorkItemAssignmentConfiguration
    : IEntityTypeConfiguration<WorkItemAssignment>
{
    public void Configure(
        EntityTypeBuilder<WorkItemAssignment> builder)
    {
        builder.ToTable("WorkItemAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WorkItemId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.AssignedByUserId)
            .HasMaxLength(450)
            .IsRequired(false);

        builder.Property(x => x.AssignedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .IsRequired(false);

        builder.HasOne<WorkItem>()
            .WithMany()
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
    .WithMany()
    .HasForeignKey(x => x.AssignedByUserId)
    .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.WorkItemId, x.UserId })
            .IsUnique();

        builder.HasIndex(x => x.UserId);
    }
}
