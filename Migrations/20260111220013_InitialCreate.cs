using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AgentPassword = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AgentOpenDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "checks",
                columns: table => new
                {
                    CheckId = table.Column<int>(type: "int", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    OrderTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sum = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransId = table.Column<int>(type: "int", nullable: false),
                    AgentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    CheckIssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checks", x => x.CheckId);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    ClientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientFirstName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ClientLastName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Phonenumber = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    ClientOpenDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Urav = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ClientPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateByEmail = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SelectedPosition = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Agent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "Updates",
                columns: table => new
                {
                    RecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ObjectId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ColumName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PrevVersion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    UpdatedVersion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Agent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Updates", x => x.RecordId);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    UpdateBalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_accounts_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    TransId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Agent = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Added = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Subtracted = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalAdded = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalSubtracted = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SendEmail = table.Column<bool>(type: "bit", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.TransId);
                    table.ForeignKey(
                        name: "FK_transactions_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_transactions_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_ClientId",
                table: "accounts",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactions_AccountId",
                table: "transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_ClientId",
                table: "transactions",
                column: "ClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "agents");

            migrationBuilder.DropTable(
                name: "checks");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "Updates");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "clients");
        }
    }
}
