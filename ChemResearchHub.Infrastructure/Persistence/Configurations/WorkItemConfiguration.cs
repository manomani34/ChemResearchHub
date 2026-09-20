using ChemResearchHub.Domain.Entities.WorkItem;
using ChemResearchHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class WorkItemConfiguration
    : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(
        EntityTypeBuilder<WorkItem> builder)
    {
        builder.ToTable("WorkItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.BoardColumnId)
            .IsRequired(false);

        builder.Property(x => x.AssignedToUserId)
            .HasMaxLength(450)
            .IsRequired(false);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Description)
            .HasMaxLength(5000);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.DueDate)
            .IsRequired(false);

        builder.Property(x => x.IsCompleted)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .IsRequired(false);

        builder.HasOne<ChemResearchHub.Domain.Entities.Project.Project>()
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ChemResearchHub.Domain.Entities.Board.BoardColumn>()
            .WithMany()
            .HasForeignKey(x => x.BoardColumnId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}