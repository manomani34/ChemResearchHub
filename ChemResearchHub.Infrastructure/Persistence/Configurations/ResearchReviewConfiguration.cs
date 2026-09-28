using ChemResearchHub.Domain.Entities.ResearchReview;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class ResearchReviewConfiguration : IEntityTypeConfiguration<ResearchReview>
{
    public void Configure(EntityTypeBuilder<ResearchReview> builder)
    {
        builder.ToTable("ResearchReviews", "pubbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.RequestedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.ReviewerUserId)
            .HasMaxLength(450);

        builder.Property(x => x.ReviewNote)
            .HasMaxLength(2000);

        builder.HasOne(x => x.WorkItem)
            .WithMany()
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.WorkItemId);
        builder.HasIndex(x => x.RequestedAtUtc);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.WorkItemId, x.Status });
    }
}
