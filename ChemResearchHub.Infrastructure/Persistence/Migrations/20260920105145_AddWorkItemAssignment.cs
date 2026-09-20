using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChemResearchHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItemAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                table: "WorkItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_AssignedToUserId",
                table: "WorkItems",
                column: "AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_AspNetUsers_AssignedToUserId",
                table: "WorkItems",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_AspNetUsers_AssignedToUserId",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_AssignedToUserId",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "WorkItems");
        }
    }
}
