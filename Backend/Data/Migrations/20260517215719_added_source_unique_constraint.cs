using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class added_source_unique_constraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_ResultList_ResultListId",
                table: "Results");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "ResultList");

            migrationBuilder.RenameColumn(
                name: "ResultListId",
                table: "Results",
                newName: "ResultByHourId");

            migrationBuilder.RenameIndex(
                name: "IX_Results_ResultListId",
                table: "Results",
                newName: "IX_Results_ResultByHourId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Assets",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ResultByHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TimeFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TimeTo = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OptimizedResultsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultByHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultByHours_OptimizedResults_OptimizedResultsId",
                        column: x => x.OptimizedResultsId,
                        principalTable: "OptimizedResults",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_TimeFrom_TimeTo_HeatDemand_ElectricityPrice",
                table: "Sources",
                columns: new[] { "TimeFrom", "TimeTo", "HeatDemand", "ElectricityPrice" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultByHours_OptimizedResultsId",
                table: "ResultByHours",
                column: "OptimizedResultsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_ResultByHours_ResultByHourId",
                table: "Results",
                column: "ResultByHourId",
                principalTable: "ResultByHours",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_ResultByHours_ResultByHourId",
                table: "Results");

            migrationBuilder.DropTable(
                name: "ResultByHours");

            migrationBuilder.DropIndex(
                name: "IX_Sources_TimeFrom_TimeTo_HeatDemand_ElectricityPrice",
                table: "Sources");

            migrationBuilder.RenameColumn(
                name: "ResultByHourId",
                table: "Results",
                newName: "ResultListId");

            migrationBuilder.RenameIndex(
                name: "IX_Results_ResultByHourId",
                table: "Results",
                newName: "IX_Results_ResultListId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Assets",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ImageLink = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ResultList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OptimizedResultsId = table.Column<int>(type: "int", nullable: true),
                    TimeFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TimeTo = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultList_OptimizedResults_OptimizedResultsId",
                        column: x => x.OptimizedResultsId,
                        principalTable: "OptimizedResults",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ResultList_OptimizedResultsId",
                table: "ResultList",
                column: "OptimizedResultsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_ResultList_ResultListId",
                table: "Results",
                column: "ResultListId",
                principalTable: "ResultList",
                principalColumn: "Id");
        }
    }
}
