using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    /// <inheritdoc />
    public partial class twentyOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Clients",
                newName: "ClientPassword");

            migrationBuilder.RenameColumn(
                name: "OpenDate",
                table: "Clients",
                newName: "ClientOpenDate");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Clients",
                newName: "ClientLastName");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Clients",
                newName: "ClientFirstName");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Agents",
                newName: "AgentPassword");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Agents",
                newName: "AgentName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientPassword",
                table: "Clients",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "ClientOpenDate",
                table: "Clients",
                newName: "OpenDate");

            migrationBuilder.RenameColumn(
                name: "ClientLastName",
                table: "Clients",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "ClientFirstName",
                table: "Clients",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "AgentPassword",
                table: "Agents",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "AgentName",
                table: "Agents",
                newName: "Name");
        }
    }
}
