using ChemResearchHub.Domain.Entities.Tag;
using ChemResearchHub.Domain.Entities.WorkItem;
using WorkItemTagEntity = ChemResearchHub.Domain.Entities.WorkItemTag.WorkItemTag;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class WorkItemTagConfiguration : IEntityTypeConfiguration<WorkItemTagEntity>
{
    public void Configure(EntityTypeBuilder<WorkItemTagEntity> builder)
    {
        builder.ToTable("WorkItemTags", "pubbo");

        builder.HasKey(x => new
        {
            x.WorkItemId,
            x.TagId
        });

        builder.Property(x => x.WorkItemId)
            .IsRequired();

        builder.Property(x => x.TagId)
            .IsRequired();

        builder.HasOne<WorkItem>()
            .WithMany(x => x.WorkItemTags)
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tag>()
            .WithMany(x => x.WorkItemTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.TagId);
    }
}


