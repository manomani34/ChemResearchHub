using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChemResearchHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pubbo");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "pubbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ResearchReviews",
                schema: "pubbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkItemId = table.Column<int>(type: "int", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReviewerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchReviews_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkItemId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BlockedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    BlockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnblockedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UnblockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnblockNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItemBlocks_AspNetUsers_BlockedByUserId",
                        column: x => x.BlockedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkItemBlocks_AspNetUsers_UnblockedByUserId",
                        column: x => x.UnblockedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkItemBlocks_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                schema: "pubbo",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                schema: "pubbo",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                schema: "pubbo",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                schema: "pubbo",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchReviews_RequestedAtUtc",
                schema: "pubbo",
                table: "ResearchReviews",
                column: "RequestedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchReviews_Status",
                schema: "pubbo",
                table: "ResearchReviews",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchReviews_WorkItemId",
                schema: "pubbo",
                table: "ResearchReviews",
                column: "WorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchReviews_WorkItemId_Status",
                schema: "pubbo",
                table: "ResearchReviews",
                columns: new[] { "WorkItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemBlocks_BlockedAtUtc",
                table: "WorkItemBlocks",
                column: "BlockedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemBlocks_BlockedByUserId",
                table: "WorkItemBlocks",
                column: "BlockedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemBlocks_UnblockedByUserId",
                table: "WorkItemBlocks",
                column: "UnblockedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemBlocks_WorkItemId",
                table: "WorkItemBlocks",
                column: "WorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemBlocks_WorkItemId_UnblockedAtUtc",
                table: "WorkItemBlocks",
                columns: new[] { "WorkItemId", "UnblockedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "pubbo");

            migrationBuilder.DropTable(
                name: "ResearchReviews",
                schema: "pubbo");

            migrationBuilder.DropTable(
                name: "WorkItemBlocks");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Projects");
        }
    }
}
