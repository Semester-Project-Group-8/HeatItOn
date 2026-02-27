using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Fifth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Sources",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Sources",
                newName: "TimeTo");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Sources",
                newName: "TimeFrom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Sources",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "TimeTo",
                table: "Sources",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "TimeFrom",
                table: "Sources",
                newName: "EndTime");
        }
    }
}
