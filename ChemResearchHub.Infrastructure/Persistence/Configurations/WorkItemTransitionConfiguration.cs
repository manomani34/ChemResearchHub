using ChemResearchHub.Domain.Entities.Board;
using ChemResearchHub.Domain.Entities.WorkItemTransition;
using ChemResearchHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class WorkItemTransitionConfiguration
    : IEntityTypeConfiguration<WorkItemTransition>
{
    public void Configure(
        EntityTypeBuilder<WorkItemTransition> builder)
    {
        builder.ToTable("WorkItemTransitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WorkItemId)
            .IsRequired();

        builder.Property(x => x.FromBoardColumnId)
            .IsRequired(false);

        builder.Property(x => x.ToBoardColumnId)
            .IsRequired();

        builder.Property(x => x.ChangedByUserId)
            .HasMaxLength(450)
            .IsRequired(false);

        builder.Property(x => x.ChangedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .IsRequired(false);

        builder.HasOne(x => x.WorkItem)
            .WithMany()
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<BoardColumn>()
            .WithMany()
            .HasForeignKey(x => x.FromBoardColumnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BoardColumn>()
            .WithMany()
            .HasForeignKey(x => x.ToBoardColumnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.WorkItemId);
        builder.HasIndex(x => x.ChangedAtUtc);
    }
}
