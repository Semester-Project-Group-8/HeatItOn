using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class fixerrors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OptimizedResultsId",
                table: "ResultList",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OptimizedResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizedResults", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ResultList_OptimizedResultsId",
                table: "ResultList",
                column: "OptimizedResultsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultList_OptimizedResults_OptimizedResultsId",
                table: "ResultList",
                column: "OptimizedResultsId",
                principalTable: "OptimizedResults",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResultList_OptimizedResults_OptimizedResultsId",
                table: "ResultList");

            migrationBuilder.DropTable(
                name: "OptimizedResults");

            migrationBuilder.DropIndex(
                name: "IX_ResultList_OptimizedResultsId",
                table: "ResultList");

            migrationBuilder.DropColumn(
                name: "OptimizedResultsId",
                table: "ResultList");
        }
    }
}
