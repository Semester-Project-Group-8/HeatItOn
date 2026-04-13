using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResultListSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResultListId",
                table: "Results",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResultList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TimeFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TimeTo = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultList", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Results_ResultListId",
                table: "Results",
                column: "ResultListId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_ResultList_ResultListId",
                table: "Results",
                column: "ResultListId",
                principalTable: "ResultList",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_ResultList_ResultListId",
                table: "Results");

            migrationBuilder.DropTable(
                name: "ResultList");

            migrationBuilder.DropIndex(
                name: "IX_Results_ResultListId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "ResultListId",
                table: "Results");
        }
    }
}
