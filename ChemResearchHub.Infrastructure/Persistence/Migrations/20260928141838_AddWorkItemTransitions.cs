using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChemResearchHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItemTransitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkItemTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkItemId = table.Column<int>(type: "int", nullable: false),
                    FromBoardColumnId = table.Column<int>(type: "int", nullable: true),
                    ToBoardColumnId = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItemTransitions_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkItemTransitions_BoardColumns_FromBoardColumnId",
                        column: x => x.FromBoardColumnId,
                        principalTable: "BoardColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItemTransitions_BoardColumns_ToBoardColumnId",
                        column: x => x.ToBoardColumnId,
                        principalTable: "BoardColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItemTransitions_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemTransitions_ChangedAtUtc",
                table: "WorkItemTransitions",
                column: "ChangedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemTransitions_ChangedByUserId",
                table: "WorkItemTransitions",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemTransitions_FromBoardColumnId",
                table: "WorkItemTransitions",
                column: "FromBoardColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemTransitions_ToBoardColumnId",
                table: "WorkItemTransitions",
                column: "ToBoardColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemTransitions_WorkItemId",
                table: "WorkItemTransitions",
                column: "WorkItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkItemTransitions");
        }
    }
}
