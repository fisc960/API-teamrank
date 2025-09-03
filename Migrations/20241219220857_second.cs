using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    /// <inheritdoc />
    public partial class Second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "totalsubtracted",
                table: "Transactions",
                newName: "Totalsubtracted");

            migrationBuilder.RenameColumn(
                name: "totaladded",
                table: "Transactions",
                newName: "Totaladded");

            migrationBuilder.RenameColumn(
                name: "subtracted",
                table: "Transactions",
                newName: "Subtracted");

            migrationBuilder.RenameColumn(
                name: "added",
                table: "Transactions",
                newName: "Added");

            migrationBuilder.RenameColumn(
                name: "phonenumber",
                table: "Clients",
                newName: "Phonenumber");

            migrationBuilder.RenameColumn(
                name: "lastName",
                table: "Clients",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "firstName",
                table: "Clients",
                newName: "FirstName");

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Clients",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Clients",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedPosition",
                table: "Clients",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Urav",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateBalDate",
                table: "Accounts",
                type: "DateTime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "SelectedPosition",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Urav",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UpdateBalDate",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "Totalsubtracted",
                table: "Transactions",
                newName: "totalsubtracted");

            migrationBuilder.RenameColumn(
                name: "Totaladded",
                table: "Transactions",
                newName: "totaladded");

            migrationBuilder.RenameColumn(
                name: "Subtracted",
                table: "Transactions",
                newName: "subtracted");

            migrationBuilder.RenameColumn(
                name: "Added",
                table: "Transactions",
                newName: "added");

            migrationBuilder.RenameColumn(
                name: "Phonenumber",
                table: "Clients",
                newName: "phonenumber");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Clients",
                newName: "lastName");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Clients",
                newName: "firstName");
        }
    }
}
