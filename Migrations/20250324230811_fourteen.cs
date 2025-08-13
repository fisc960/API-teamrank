using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    /// <inheritdoc />
    public partial class Fourteen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SendEmail",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendEmail",
                table: "Transactions");
        }
    }
}
