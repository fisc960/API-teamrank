IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251117013640_InitialSqlServer', N'6.0.36');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251209013944_FixTableNames', N'6.0.36');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251209015327_FixPostgreSQLTableNames', N'6.0.36');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251212000003_InitialCreate', N'6.0.36');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [accounts] DROP CONSTRAINT [FK_accounts_clients_clientid];
GO

ALTER TABLE [transactions] DROP CONSTRAINT [FK_transactions_accounts_accountid];
GO

ALTER TABLE [transactions] DROP CONSTRAINT [FK_transactions_clients_clientid];
GO

ALTER TABLE [updates] DROP CONSTRAINT [PK_updates];
GO

ALTER TABLE [admins] DROP CONSTRAINT [PK_admins];
GO

EXEC sp_rename N'[updates]', N'Updates';
GO

EXEC sp_rename N'[admins]', N'Admins';
GO

EXEC sp_rename N'[Updates].[updatedversion]', N'UpdatedVersion', N'COLUMN';
GO

EXEC sp_rename N'[Updates].[timestamp]', N'Timestamp', N'COLUMN';
GO

EXEC sp_rename N'[Updates].[tablename]', N'TableName', N'COLUMN';
GO

EXEC sp_rename N'[Updates].[prevversion]', N'PrevVersion', N'COLUMN';
GO

EXEC sp_rename N'[Updates].[objectid]', N'ObjectId', N'COLUMN';
GO

EXEC sp_rename N'[Updates].[columname]', N'ColumName', N'COLUMN';
GO

EXEC sp_rename N'[Updates].[agent]', N'Agent', N'COLUMN';
GO

EXEC sp_rename N'[Updates].[recordid]', N'RecordId', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[transdate]', N'TransDate', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[totalsubtracted]', N'TotalSubtracted', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[totaladded]', N'TotalAdded', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[subtracted]', N'Subtracted', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[sendemail]', N'SendEmail', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[clientid]', N'ClientId', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[agent]', N'Agent', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[added]', N'Added', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[accountid]', N'AccountId', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[transid]', N'TransId', N'COLUMN';
GO

EXEC sp_rename N'[transactions].[IX_transactions_clientid]', N'IX_transactions_ClientId', N'INDEX';
GO

EXEC sp_rename N'[transactions].[IX_transactions_accountid]', N'IX_transactions_AccountId', N'INDEX';
GO

EXEC sp_rename N'[clients].[urav]', N'Urav', N'COLUMN';
GO

EXEC sp_rename N'[clients].[updatebyemail]', N'UpdateByEmail', N'COLUMN';
GO

EXEC sp_rename N'[clients].[selectedposition]', N'SelectedPosition', N'COLUMN';
GO

EXEC sp_rename N'[clients].[phonenumber]', N'Phonenumber', N'COLUMN';
GO

EXEC sp_rename N'[clients].[email]', N'Email', N'COLUMN';
GO

EXEC sp_rename N'[clients].[comments]', N'Comments', N'COLUMN';
GO

EXEC sp_rename N'[clients].[clientpassword]', N'ClientPassword', N'COLUMN';
GO

EXEC sp_rename N'[clients].[clientopendate]', N'ClientOpenDate', N'COLUMN';
GO

EXEC sp_rename N'[clients].[clientlastname]', N'ClientLastName', N'COLUMN';
GO

EXEC sp_rename N'[clients].[clientfirstname]', N'ClientFirstName', N'COLUMN';
GO

EXEC sp_rename N'[clients].[agent]', N'Agent', N'COLUMN';
GO

EXEC sp_rename N'[clients].[clientid]', N'ClientId', N'COLUMN';
GO

EXEC sp_rename N'[checks].[transid]', N'TransId', N'COLUMN';
GO

EXEC sp_rename N'[checks].[sum]', N'Sum', N'COLUMN';
GO

EXEC sp_rename N'[checks].[orderto]', N'OrderTo', N'COLUMN';
GO

EXEC sp_rename N'[checks].[clientname]', N'ClientName', N'COLUMN';
GO

EXEC sp_rename N'[checks].[clientid]', N'ClientId', N'COLUMN';
GO

EXEC sp_rename N'[checks].[checkissueddate]', N'CheckIssuedDate', N'COLUMN';
GO

EXEC sp_rename N'[checks].[agentname]', N'AgentName', N'COLUMN';
GO

EXEC sp_rename N'[checks].[agentid]', N'AgentId', N'COLUMN';
GO

EXEC sp_rename N'[checks].[checkid]', N'CheckId', N'COLUMN';
GO

EXEC sp_rename N'[agents].[agentpassword]', N'AgentPassword', N'COLUMN';
GO

EXEC sp_rename N'[agents].[agentopendate]', N'AgentOpenDate', N'COLUMN';
GO

EXEC sp_rename N'[agents].[agentname]', N'AgentName', N'COLUMN';
GO

EXEC sp_rename N'[agents].[id]', N'Id', N'COLUMN';
GO

EXEC sp_rename N'[Admins].[passwordhash]', N'PasswordHash', N'COLUMN';
GO

EXEC sp_rename N'[Admins].[password]', N'Password', N'COLUMN';
GO

EXEC sp_rename N'[Admins].[id]', N'Id', N'COLUMN';
GO

EXEC sp_rename N'[accounts].[updatebaldate]', N'UpdateBalDate', N'COLUMN';
GO

EXEC sp_rename N'[accounts].[totalamount]', N'TotalAmount', N'COLUMN';
GO

EXEC sp_rename N'[accounts].[clientid]', N'ClientId', N'COLUMN';
GO

EXEC sp_rename N'[accounts].[accountid]', N'AccountId', N'COLUMN';
GO

EXEC sp_rename N'[accounts].[IX_accounts_clientid]', N'IX_accounts_ClientId', N'INDEX';
GO

ALTER TABLE [Admins] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Updates] ADD CONSTRAINT [PK_Updates] PRIMARY KEY ([RecordId]);
GO

ALTER TABLE [Admins] ADD CONSTRAINT [PK_Admins] PRIMARY KEY ([Id]);
GO

ALTER TABLE [accounts] ADD CONSTRAINT [FK_accounts_clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [clients] ([ClientId]) ON DELETE CASCADE;
GO

ALTER TABLE [transactions] ADD CONSTRAINT [FK_transactions_accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [accounts] ([AccountId]);
GO

ALTER TABLE [transactions] ADD CONSTRAINT [FK_transactions_clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [clients] ([ClientId]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251226005032_second', N'6.0.36');
GO

COMMIT;
GO

