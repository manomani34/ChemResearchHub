using ChemResearchHub.Domain.Entities.WorkItemBlock;
using ChemResearchHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class WorkItemBlockConfiguration : IEntityTypeConfiguration<WorkItemBlock>
{
    public void Configure(EntityTypeBuilder<WorkItemBlock> builder)
    {
        builder.ToTable("WorkItemBlocks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.WorkItemId).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.BlockedByUserId).HasMaxLength(450).IsRequired(false);
        builder.Property(x => x.BlockedAtUtc).IsRequired();
        builder.Property(x => x.UnblockedByUserId).HasMaxLength(450).IsRequired(false);
        builder.Property(x => x.UnblockedAtUtc).IsRequired(false);
        builder.Property(x => x.UnblockNote).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifiedAt).IsRequired(false);

        builder.HasOne(x => x.WorkItem)
            .WithMany()
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.BlockedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UnblockedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.WorkItemId);
        builder.HasIndex(x => new { x.WorkItemId, x.UnblockedAtUtc });
        builder.HasIndex(x => x.BlockedAtUtc);
    }
}
