using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    /// <inheritdoc />
    public partial class Nineteen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Agent",
                table: "Clients",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Updates",
                columns: table => new
                {
                    RecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "varchar(max)", nullable: false),
                    ColumName = table.Column<string>(type: "varchar(max)", nullable: false),
                    PrevVersion = table.Column<string>(type: "varchar(max)", nullable: false),
                    UpdatedVersion = table.Column<string>(type: "varchar(max)", nullable: false),
                    Agent = table.Column<string>(type: "varchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "DateTime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Updates", x => x.RecordId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Updates");

            migrationBuilder.DropColumn(
                name: "Agent",
                table: "Clients");
        }
    }
}
