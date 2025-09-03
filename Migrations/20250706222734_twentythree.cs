using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    /// <inheritdoc />
    public partial class Twentythree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClientLastName",
                table: "Clients",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "AgentOpenDate",
                table: "Agents",
                type: "DateTime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Checks",
                columns: table => new
                {
                    checkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientName = table.Column<string>(type: "varchar(max)", nullable: false),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    orderTo = table.Column<string>(type: "varchar(max)", nullable: false),
                    sum = table.Column<int>(type: "int", nullable: false),
                    TransId = table.Column<int>(type: "int", nullable: false),
                    agentName = table.Column<string>(type: "varchar(max)", nullable: false),
                    agentId = table.Column<int>(type: "int", nullable: false),
                    CheckIssuedDate = table.Column<DateTime>(type: "DateTime", nullable: false)
                },  
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checks", x => x.checkId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checks");

            migrationBuilder.DropColumn(
                name: "AgentOpenDate",
                table: "Agents");

            migrationBuilder.AlterColumn<string>(
                name: "ClientLastName",
                table: "Clients",
                type: "varchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40);
        }
    }
}
