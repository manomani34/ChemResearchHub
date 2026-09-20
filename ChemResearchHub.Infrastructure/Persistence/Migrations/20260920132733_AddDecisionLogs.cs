using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChemResearchHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDecisionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DecisionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkItemId = table.Column<int>(type: "int", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Evidence = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecisionLogs_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DecisionLogs_CreatedAt",
                table: "DecisionLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionLogs_WorkItemId",
                table: "DecisionLogs",
                column: "WorkItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecisionLogs");
        }
    }
}
