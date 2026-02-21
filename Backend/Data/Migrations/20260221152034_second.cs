using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Demand",
                table: "Demand");

            migrationBuilder.RenameTable(
                name: "Demand",
                newName: "Demands");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Demands",
                table: "Demands",
                column: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Demands",
                table: "Demands");

            migrationBuilder.RenameTable(
                name: "Demands",
                newName: "Demand");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Demand",
                table: "Demand",
                column: "ID");
        }
    }
}
