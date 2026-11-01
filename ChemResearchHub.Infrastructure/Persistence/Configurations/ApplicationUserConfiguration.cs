using ChemResearchHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.BaleChatId)
            .HasColumnType("bigint");

        builder.HasIndex(x => x.BaleChatId)
            .IsUnique()
            .HasFilter("[BaleChatId] IS NOT NULL");
    }
}
