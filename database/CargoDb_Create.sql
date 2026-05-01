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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429162349_InitialCreate')
BEGIN
    CREATE TABLE [Carriers] (
        [CarrierId] int NOT NULL IDENTITY,
        [CarrierName] nvarchar(max) NOT NULL,
        [CarrierIsActive] bit NOT NULL,
        [CarrierPlusDesiCost] int NOT NULL,
        [CarrierConfigurationId] int NOT NULL,
        CONSTRAINT [PK_Carriers] PRIMARY KEY ([CarrierId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429162349_InitialCreate')
BEGIN
    CREATE TABLE [CarrierConfigurations] (
        [CarrierConfigurationId] int NOT NULL IDENTITY,
        [CarrierId] int NOT NULL,
        [CarrierMaxDesi] int NOT NULL,
        [CarrierMinDesi] int NOT NULL,
        [CarrierCost] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_CarrierConfigurations] PRIMARY KEY ([CarrierConfigurationId]),
        CONSTRAINT [FK_CarrierConfigurations_Carriers_CarrierId] FOREIGN KEY ([CarrierId]) REFERENCES [Carriers] ([CarrierId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429162349_InitialCreate')
BEGIN
    CREATE TABLE [Orders] (
        [OrderId] int NOT NULL IDENTITY,
        [OrderDesi] int NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [OrderCarrierCost] decimal(18,2) NOT NULL,
        [CarrierId] int NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderId]),
        CONSTRAINT [FK_Orders_Carriers_CarrierId] FOREIGN KEY ([CarrierId]) REFERENCES [Carriers] ([CarrierId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429162349_InitialCreate')
BEGIN
    CREATE INDEX [IX_CarrierConfigurations_CarrierId] ON [CarrierConfigurations] ([CarrierId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429162349_InitialCreate')
BEGIN
    CREATE INDEX [IX_Orders_CarrierId] ON [Orders] ([CarrierId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429162349_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429162349_InitialCreate', N'6.0.36');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429175618_AddCarrierReports')
BEGIN
    CREATE TABLE [CarrierReports] (
        [CarrierReportId] int NOT NULL IDENTITY,
        [CarrierId] int NOT NULL,
        [CarrierCost] decimal(18,2) NOT NULL,
        [CarrierReportDate] datetime2 NOT NULL,
        CONSTRAINT [PK_CarrierReports] PRIMARY KEY ([CarrierReportId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429175618_AddCarrierReports')
BEGIN
    CREATE UNIQUE INDEX [IX_CarrierReports_CarrierId_CarrierReportDate] ON [CarrierReports] ([CarrierId], [CarrierReportDate]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429175618_AddCarrierReports')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429175618_AddCarrierReports', N'6.0.36');
END;
GO

COMMIT;
GO

