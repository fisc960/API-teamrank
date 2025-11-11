using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    public partial class InitialPostgres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    passwordhash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agents",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    agentname = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    agentpassword = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    agentopendate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "checks",
                columns: table => new
                {
                    checkid = table.Column<int>(type: "int", nullable: false),
                    clientname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    clientid = table.Column<int>(type: "int", nullable: false),
                    orderto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sum = table.Column<int>(type: "int", nullable: false),
                    transid = table.Column<int>(type: "int", nullable: false),
                    agentname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    agentid = table.Column<int>(type: "int", nullable: false),
                    checkissueddate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checks", x => x.checkid);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    clientid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientfirstname = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    clientlastname = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    phonenumber = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    clientopendate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    urav = table.Column<bool>(type: "bit", nullable: false),
                    comments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    clientpassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatebyemail = table.Column<bool>(type: "bit", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    selectedposition = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    agent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.clientid);
                });

            migrationBuilder.CreateTable(
                name: "updates",
                columns: table => new
                {
                    recordid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tablename = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    objectid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    columname = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    prevversion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    updatedversion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    agent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_updates", x => x.recordid);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    accountid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientid = table.Column<int>(type: "int", nullable: false),
                    updatebaldate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    totalamount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.accountid);
                    table.ForeignKey(
                        name: "FK_accounts_clients_clientid",
                        column: x => x.clientid,
                        principalTable: "clients",
                        principalColumn: "clientid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactios",
                columns: table => new
                {
                    transid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientid = table.Column<int>(type: "int", nullable: false),
                    transdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    agent = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    added = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    subtracted = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    totaladded = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    totalsubtracted = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    sendemail = table.Column<bool>(type: "bit", nullable: false),
                    accountid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactios", x => x.transid);
                    table.ForeignKey(
                        name: "FK_transactios_accounts_accountid",
                        column: x => x.accountid,
                        principalTable: "accounts",
                        principalColumn: "accountid");
                    table.ForeignKey(
                        name: "FK_transactios_clients_clientid",
                        column: x => x.clientid,
                        principalTable: "clients",
                        principalColumn: "clientid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_clientid",
                table: "accounts",
                column: "clientid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactios_accountid",
                table: "transactios",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_transactios_clientid",
                table: "transactios",
                column: "clientid");
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
                name: "transactios");

            migrationBuilder.DropTable(
                name: "updates");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "clients");
        }
    }
}
