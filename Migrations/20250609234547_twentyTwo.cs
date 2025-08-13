using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    /// <inheritdoc />
    public partial class twentyTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UpdateByEmail",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateByEmail",
                table: "Clients");
        }
    }
}
