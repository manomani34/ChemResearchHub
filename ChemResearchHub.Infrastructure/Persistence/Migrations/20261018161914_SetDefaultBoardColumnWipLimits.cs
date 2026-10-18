using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChemResearchHub.Infrastructure.Persistence.Migrations
{
    public partial class SetDefaultBoardColumnWipLimits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE BoardColumns
                SET WipLimit = 5
                WHERE Name = 'Ready'
                  AND WipLimit IS NULL;

                UPDATE BoardColumns
                SET WipLimit = 4
                WHERE Name = 'In Progress'
                  AND WipLimit IS NULL;

                UPDATE BoardColumns
                SET WipLimit = 3
                WHERE Name = 'Review / Validation'
                  AND WipLimit IS NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE BoardColumns
                SET WipLimit = NULL
                WHERE Name IN ('Ready', 'In Progress', 'Review / Validation')
                  AND WipLimit IN (3, 4, 5);
                """);
        }
    }
}