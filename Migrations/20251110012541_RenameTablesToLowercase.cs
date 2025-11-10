using Microsoft.EntityFrameworkCore.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

#nullable disable

namespace GemachApp.Migrations
{
    public partial class RenameTablesToLowercase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Agents\" RENAME TO agents;");
            migrationBuilder.Sql("ALTER TABLE \"Clients\" RENAME TO clients;");
            migrationBuilder.Sql("ALTER TABLE \"Transactions\" RENAME TO transactions;");
            migrationBuilder.Sql("ALTER TABLE \"Accounts\" RENAME TO accounts;");
            migrationBuilder.Sql("ALTER TABLE \"Checks\" RENAME TO checks;");
            migrationBuilder.Sql("ALTER TABLE \"Admins\" RENAME TO admins;");
            migrationBuilder.Sql("ALTER TABLE \"Updates\" RENAME TO updates;");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE agents RENAME TO \"Agents\";");
            migrationBuilder.Sql("ALTER TABLE clients RENAME TO \"Clients\";");
            migrationBuilder.Sql("ALTER TABLE transactions RENAME TO \"Transactions\";");
            migrationBuilder.Sql("ALTER TABLE accounts RENAME TO \"Accounts\";");
            migrationBuilder.Sql("ALTER TABLE checks RENAME TO \"Checks\";");
            migrationBuilder.Sql("ALTER TABLE admins RENAME TO \"Admins\";");
            migrationBuilder.Sql("ALTER TABLE updates RENAME TO \"Updates\";");
        }

    }
}
