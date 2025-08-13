using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    /// <inheritdoc />
    public partial class Fifth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Totalsubtracted",
                table: "Transactions",
                newName: "TotalSubtracted");

            migrationBuilder.RenameColumn(
                name: "Totaladded",
                table: "Transactions",
                newName: "TotalAdded");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalSubtracted",
                table: "Transactions",
                newName: "Totalsubtracted");

            migrationBuilder.RenameColumn(
                name: "TotalAdded",
                table: "Transactions",
                newName: "Totaladded");
        }
    }
}
