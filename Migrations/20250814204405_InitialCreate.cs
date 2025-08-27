using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sum",
                table: "Checks",
                newName: "Sum");

            migrationBuilder.RenameColumn(
                name: "orderTo",
                table: "Checks",
                newName: "OrderTo");

            migrationBuilder.RenameColumn(
                name: "clientId",
                table: "Checks",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "agentName",
                table: "Checks",
                newName: "AgentName");

            migrationBuilder.RenameColumn(
                name: "agentId",
                table: "Checks",
                newName: "AgentId");

            migrationBuilder.RenameColumn(
                name: "checkId",
                table: "Checks",
                newName: "CheckId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sum",
                table: "Checks",
                newName: "sum");

            migrationBuilder.RenameColumn(
                name: "OrderTo",
                table: "Checks",
                newName: "orderTo");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Checks",
                newName: "clientId");

            migrationBuilder.RenameColumn(
                name: "AgentName",
                table: "Checks",
                newName: "agentName");

            migrationBuilder.RenameColumn(
                name: "AgentId",
                table: "Checks",
                newName: "agentId");

            migrationBuilder.RenameColumn(
                name: "CheckId",
                table: "Checks",
                newName: "checkId");
        }
    }
}
