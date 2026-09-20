using ChemResearchHub.Domain.Entities.Board;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class BoardColumnConfiguration
    : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(
        EntityTypeBuilder<BoardColumn> builder)
    {
        builder.ToTable("BoardColumns");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BoardId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.WipLimit)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .IsRequired(false);
    }
}