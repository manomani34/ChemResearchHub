using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChemResearchHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Samples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExperimentId = table.Column<int>(type: "int", nullable: false),
                    SampleCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SampleType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Matrix = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PreparationMethod = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CollectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalReference = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Samples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Samples_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Samples_ExperimentId",
                table: "Samples",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_Samples_SampleCode",
                table: "Samples",
                column: "SampleCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Samples");
        }
    }
}
