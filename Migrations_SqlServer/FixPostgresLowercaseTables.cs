using Microsoft.EntityFrameworkCore.Migrations;


namespace GemachApp.Data.Migrations
{
    public partial class FixPostgresLowercaseTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename tables to lowercase
            migrationBuilder.RenameTable(name: "Clients", newName: "clients");
            migrationBuilder.RenameTable(name: "Transactions", newName: "transactions");
            migrationBuilder.RenameTable(name: "Accounts", newName: "accounts");
            migrationBuilder.RenameTable(name: "Admins", newName: "admins");
            migrationBuilder.RenameTable(name: "Agents", newName: "agents");
            migrationBuilder.RenameTable(name: "Updates", newName: "updates");
            migrationBuilder.RenameTable(name: "Checks", newName: "checks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert table names to original
            migrationBuilder.RenameTable(name: "clients", newName: "Clients");
            migrationBuilder.RenameTable(name: "transactions", newName: "Transactions");
            migrationBuilder.RenameTable(name: "accounts", newName: "Accounts");
            migrationBuilder.RenameTable(name: "admins", newName: "Admins");
            migrationBuilder.RenameTable(name: "agents", newName: "Agents");
            migrationBuilder.RenameTable(name: "updates", newName: "Updates");
            migrationBuilder.RenameTable(name: "checks", newName: "Checks");
        }
    }
}