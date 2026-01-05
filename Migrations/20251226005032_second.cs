using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemachApp.Migrations
{
    public partial class second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_clients_clientid",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_accounts_accountid",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_clients_clientid",
                table: "transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_updates",
                table: "updates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_admins",
                table: "admins");

            migrationBuilder.RenameTable(
                name: "updates",
                newName: "Updates");

            migrationBuilder.RenameTable(
                name: "admins",
                newName: "Admins");

            migrationBuilder.RenameColumn(
                name: "updatedversion",
                table: "Updates",
                newName: "UpdatedVersion");

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "Updates",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "tablename",
                table: "Updates",
                newName: "TableName");

            migrationBuilder.RenameColumn(
                name: "prevversion",
                table: "Updates",
                newName: "PrevVersion");

            migrationBuilder.RenameColumn(
                name: "objectid",
                table: "Updates",
                newName: "ObjectId");

            migrationBuilder.RenameColumn(
                name: "columname",
                table: "Updates",
                newName: "ColumName");

            migrationBuilder.RenameColumn(
                name: "agent",
                table: "Updates",
                newName: "Agent");

            migrationBuilder.RenameColumn(
                name: "recordid",
                table: "Updates",
                newName: "RecordId");

            migrationBuilder.RenameColumn(
                name: "transdate",
                table: "transactions",
                newName: "TransDate");

            migrationBuilder.RenameColumn(
                name: "totalsubtracted",
                table: "transactions",
                newName: "TotalSubtracted");

            migrationBuilder.RenameColumn(
                name: "totaladded",
                table: "transactions",
                newName: "TotalAdded");

            migrationBuilder.RenameColumn(
                name: "subtracted",
                table: "transactions",
                newName: "Subtracted");

            migrationBuilder.RenameColumn(
                name: "sendemail",
                table: "transactions",
                newName: "SendEmail");

            migrationBuilder.RenameColumn(
                name: "clientid",
                table: "transactions",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "agent",
                table: "transactions",
                newName: "Agent");

            migrationBuilder.RenameColumn(
                name: "added",
                table: "transactions",
                newName: "Added");

            migrationBuilder.RenameColumn(
                name: "accountid",
                table: "transactions",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "transid",
                table: "transactions",
                newName: "TransId");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_clientid",
                table: "transactions",
                newName: "IX_transactions_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_accountid",
                table: "transactions",
                newName: "IX_transactions_AccountId");

            migrationBuilder.RenameColumn(
                name: "urav",
                table: "clients",
                newName: "Urav");

            migrationBuilder.RenameColumn(
                name: "updatebyemail",
                table: "clients",
                newName: "UpdateByEmail");

            migrationBuilder.RenameColumn(
                name: "selectedposition",
                table: "clients",
                newName: "SelectedPosition");

            migrationBuilder.RenameColumn(
                name: "phonenumber",
                table: "clients",
                newName: "Phonenumber");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "clients",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "comments",
                table: "clients",
                newName: "Comments");

            migrationBuilder.RenameColumn(
                name: "clientpassword",
                table: "clients",
                newName: "ClientPassword");

            migrationBuilder.RenameColumn(
                name: "clientopendate",
                table: "clients",
                newName: "ClientOpenDate");

            migrationBuilder.RenameColumn(
                name: "clientlastname",
                table: "clients",
                newName: "ClientLastName");

            migrationBuilder.RenameColumn(
                name: "clientfirstname",
                table: "clients",
                newName: "ClientFirstName");

            migrationBuilder.RenameColumn(
                name: "agent",
                table: "clients",
                newName: "Agent");

            migrationBuilder.RenameColumn(
                name: "clientid",
                table: "clients",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "transid",
                table: "checks",
                newName: "TransId");

            migrationBuilder.RenameColumn(
                name: "sum",
                table: "checks",
                newName: "Sum");

            migrationBuilder.RenameColumn(
                name: "orderto",
                table: "checks",
                newName: "OrderTo");

            migrationBuilder.RenameColumn(
                name: "clientname",
                table: "checks",
                newName: "ClientName");

            migrationBuilder.RenameColumn(
                name: "clientid",
                table: "checks",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "checkissueddate",
                table: "checks",
                newName: "CheckIssuedDate");

            migrationBuilder.RenameColumn(
                name: "agentname",
                table: "checks",
                newName: "AgentName");

            migrationBuilder.RenameColumn(
                name: "agentid",
                table: "checks",
                newName: "AgentId");

            migrationBuilder.RenameColumn(
                name: "checkid",
                table: "checks",
                newName: "CheckId");

            migrationBuilder.RenameColumn(
                name: "agentpassword",
                table: "agents",
                newName: "AgentPassword");

            migrationBuilder.RenameColumn(
                name: "agentopendate",
                table: "agents",
                newName: "AgentOpenDate");

            migrationBuilder.RenameColumn(
                name: "agentname",
                table: "agents",
                newName: "AgentName");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "agents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "passwordhash",
                table: "Admins",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Admins",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Admins",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updatebaldate",
                table: "accounts",
                newName: "UpdateBalDate");

            migrationBuilder.RenameColumn(
                name: "totalamount",
                table: "accounts",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "clientid",
                table: "accounts",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "accountid",
                table: "accounts",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_clientid",
                table: "accounts",
                newName: "IX_accounts_ClientId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Updates",
                table: "Updates",
                column: "RecordId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Admins",
                table: "Admins",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_clients_ClientId",
                table: "accounts",
                column: "ClientId",
                principalTable: "clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_accounts_AccountId",
                table: "transactions",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_clients_ClientId",
                table: "transactions",
                column: "ClientId",
                principalTable: "clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_clients_ClientId",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_accounts_AccountId",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_clients_ClientId",
                table: "transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Updates",
                table: "Updates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Admins",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Admins");

            migrationBuilder.RenameTable(
                name: "Updates",
                newName: "updates");

            migrationBuilder.RenameTable(
                name: "Admins",
                newName: "admins");

            migrationBuilder.RenameColumn(
                name: "UpdatedVersion",
                table: "updates",
                newName: "updatedversion");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "updates",
                newName: "timestamp");

            migrationBuilder.RenameColumn(
                name: "TableName",
                table: "updates",
                newName: "tablename");

            migrationBuilder.RenameColumn(
                name: "PrevVersion",
                table: "updates",
                newName: "prevversion");

            migrationBuilder.RenameColumn(
                name: "ObjectId",
                table: "updates",
                newName: "objectid");

            migrationBuilder.RenameColumn(
                name: "ColumName",
                table: "updates",
                newName: "columname");

            migrationBuilder.RenameColumn(
                name: "Agent",
                table: "updates",
                newName: "agent");

            migrationBuilder.RenameColumn(
                name: "RecordId",
                table: "updates",
                newName: "recordid");

            migrationBuilder.RenameColumn(
                name: "TransDate",
                table: "transactions",
                newName: "transdate");

            migrationBuilder.RenameColumn(
                name: "TotalSubtracted",
                table: "transactions",
                newName: "totalsubtracted");

            migrationBuilder.RenameColumn(
                name: "TotalAdded",
                table: "transactions",
                newName: "totaladded");

            migrationBuilder.RenameColumn(
                name: "Subtracted",
                table: "transactions",
                newName: "subtracted");

            migrationBuilder.RenameColumn(
                name: "SendEmail",
                table: "transactions",
                newName: "sendemail");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "transactions",
                newName: "clientid");

            migrationBuilder.RenameColumn(
                name: "Agent",
                table: "transactions",
                newName: "agent");

            migrationBuilder.RenameColumn(
                name: "Added",
                table: "transactions",
                newName: "added");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "transactions",
                newName: "accountid");

            migrationBuilder.RenameColumn(
                name: "TransId",
                table: "transactions",
                newName: "transid");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_ClientId",
                table: "transactions",
                newName: "IX_transactions_clientid");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_AccountId",
                table: "transactions",
                newName: "IX_transactions_accountid");

            migrationBuilder.RenameColumn(
                name: "Urav",
                table: "clients",
                newName: "urav");

            migrationBuilder.RenameColumn(
                name: "UpdateByEmail",
                table: "clients",
                newName: "updatebyemail");

            migrationBuilder.RenameColumn(
                name: "SelectedPosition",
                table: "clients",
                newName: "selectedposition");

            migrationBuilder.RenameColumn(
                name: "Phonenumber",
                table: "clients",
                newName: "phonenumber");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "clients",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "clients",
                newName: "comments");

            migrationBuilder.RenameColumn(
                name: "ClientPassword",
                table: "clients",
                newName: "clientpassword");

            migrationBuilder.RenameColumn(
                name: "ClientOpenDate",
                table: "clients",
                newName: "clientopendate");

            migrationBuilder.RenameColumn(
                name: "ClientLastName",
                table: "clients",
                newName: "clientlastname");

            migrationBuilder.RenameColumn(
                name: "ClientFirstName",
                table: "clients",
                newName: "clientfirstname");

            migrationBuilder.RenameColumn(
                name: "Agent",
                table: "clients",
                newName: "agent");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "clients",
                newName: "clientid");

            migrationBuilder.RenameColumn(
                name: "TransId",
                table: "checks",
                newName: "transid");

            migrationBuilder.RenameColumn(
                name: "Sum",
                table: "checks",
                newName: "sum");

            migrationBuilder.RenameColumn(
                name: "OrderTo",
                table: "checks",
                newName: "orderto");

            migrationBuilder.RenameColumn(
                name: "ClientName",
                table: "checks",
                newName: "clientname");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "checks",
                newName: "clientid");

            migrationBuilder.RenameColumn(
                name: "CheckIssuedDate",
                table: "checks",
                newName: "checkissueddate");

            migrationBuilder.RenameColumn(
                name: "AgentName",
                table: "checks",
                newName: "agentname");

            migrationBuilder.RenameColumn(
                name: "AgentId",
                table: "checks",
                newName: "agentid");

            migrationBuilder.RenameColumn(
                name: "CheckId",
                table: "checks",
                newName: "checkid");

            migrationBuilder.RenameColumn(
                name: "AgentPassword",
                table: "agents",
                newName: "agentpassword");

            migrationBuilder.RenameColumn(
                name: "AgentOpenDate",
                table: "agents",
                newName: "agentopendate");

            migrationBuilder.RenameColumn(
                name: "AgentName",
                table: "agents",
                newName: "agentname");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "agents",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "admins",
                newName: "passwordhash");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "admins",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "admins",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdateBalDate",
                table: "accounts",
                newName: "updatebaldate");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "accounts",
                newName: "totalamount");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "accounts",
                newName: "clientid");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "accounts",
                newName: "accountid");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_ClientId",
                table: "accounts",
                newName: "IX_accounts_clientid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_updates",
                table: "updates",
                column: "recordid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_admins",
                table: "admins",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_clients_clientid",
                table: "accounts",
                column: "clientid",
                principalTable: "clients",
                principalColumn: "clientid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_accounts_accountid",
                table: "transactions",
                column: "accountid",
                principalTable: "accounts",
                principalColumn: "accountid");

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_clients_clientid",
                table: "transactions",
                column: "clientid",
                principalTable: "clients",
                principalColumn: "clientid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
