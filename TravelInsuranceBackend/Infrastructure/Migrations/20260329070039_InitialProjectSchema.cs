using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialProjectSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Identity Tables (Skip if exist)
            migrationBuilder.Sql("IF OBJECT_ID('AspNetRoles', 'U') IS NULL " + 
                "CREATE TABLE [AspNetRoles] ( [Id] nvarchar(450) NOT NULL, [Name] nvarchar(256) NULL, [NormalizedName] nvarchar(256) NULL, [ConcurrencyStamp] nvarchar(max) NULL, CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id]) );");
            
            migrationBuilder.Sql("IF OBJECT_ID('AspNetUsers', 'U') IS NULL " + 
                "CREATE TABLE [AspNetUsers] ( [Id] nvarchar(450) NOT NULL, [FullName] nvarchar(max) NOT NULL, [Role] nvarchar(max) NOT NULL, [IsActive] bit NOT NULL, [CreatedAt] datetime2 NOT NULL, [UserName] nvarchar(256) NULL, [NormalizedUserName] nvarchar(256) NULL, [Email] nvarchar(256) NULL, [NormalizedEmail] nvarchar(256) NULL, [EmailConfirmed] bit NOT NULL, [PasswordHash] nvarchar(max) NULL, [SecurityStamp] nvarchar(max) NULL, [ConcurrencyStamp] nvarchar(max) NULL, [PhoneNumber] nvarchar(max) NULL, [PhoneNumberConfirmed] bit NOT NULL, [TwoFactorEnabled] bit NOT NULL, [LockoutEnd] datetimeoffset NULL, [LockoutEnabled] bit NOT NULL, [AccessFailedCount] int NOT NULL, CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]) );");

            // CountryRisks Refactor (Surgical)
            migrationBuilder.Sql(@"
                IF OBJECT_ID('CountryRisks', 'U') IS NULL
                BEGIN
                    CREATE TABLE [CountryRisks] (
                        [Id] int NOT NULL IDENTITY(1, 1),
                        [Name] nvarchar(100) NOT NULL,
                        [Multiplier] decimal(18,2) NOT NULL,
                        [IsActive] bit NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_CountryRisks] PRIMARY KEY ([Id])
                    );
                END
                ELSE
                BEGIN
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CountryRisks') AND name = 'RiskFactor')
                    BEGIN
                        EXEC sp_rename 'CountryRisks.RiskFactor', 'Multiplier', 'COLUMN';
                    END
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CountryRisks') AND name = 'FlagEmoji')
                    BEGIN
                        ALTER TABLE CountryRisks DROP COLUMN FlagEmoji;
                    END
                    ALTER TABLE CountryRisks ALTER COLUMN Multiplier decimal(18,2) NOT NULL;
                END");

            // PolicyProducts (Skip if exist)
            migrationBuilder.Sql("IF OBJECT_ID('PolicyProducts', 'U') IS NULL " + 
                "CREATE TABLE [PolicyProducts] ( [PolicyProductId] int NOT NULL IDENTITY(1, 1), [PolicyName] nvarchar(max) NOT NULL, [PolicyType] nvarchar(max) NOT NULL, [PlanTier] nvarchar(max) NOT NULL, [CoverageDetails] nvarchar(max) NOT NULL, [ExclusionDetails] nvarchar(max) NOT NULL, [CoverageLimit] decimal(18,2) NOT NULL, [BasePremium] decimal(18,2) NOT NULL, [Tenure] int NOT NULL, [ClaimLimit] decimal(18,2) NOT NULL, [DestinationZone] nvarchar(max) NOT NULL, [Status] int NOT NULL, [CreatedAt] datetime2 NOT NULL, CONSTRAINT [PK_PolicyProducts] PRIMARY KEY ([PolicyProductId]) );");

            // ... Other tables can be handled similarly if they exist ...
            // For now, I'll ensure the rest of the Identity tables are safely handled
            migrationBuilder.Sql("IF OBJECT_ID('AspNetRoleClaims', 'U') IS NULL CREATE TABLE [AspNetRoleClaims] ( [Id] int NOT NULL IDENTITY(1, 1), [RoleId] nvarchar(450) NOT NULL, [ClaimType] nvarchar(max) NULL, [ClaimValue] nvarchar(max) NULL, CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]), CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE );");
            migrationBuilder.Sql("IF OBJECT_ID('AspNetUserClaims', 'U') IS NULL CREATE TABLE [AspNetUserClaims] ( [Id] int NOT NULL IDENTITY(1, 1), [UserId] nvarchar(450) NOT NULL, [ClaimType] nvarchar(max) NULL, [ClaimValue] nvarchar(max) NULL, CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]), CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE );");
            migrationBuilder.Sql("IF OBJECT_ID('AspNetUserLogins', 'U') IS NULL CREATE TABLE [AspNetUserLogins] ( [LoginProvider] nvarchar(450) NOT NULL, [ProviderKey] nvarchar(450) NOT NULL, [ProviderDisplayName] nvarchar(max) NULL, [UserId] nvarchar(450) NOT NULL, CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]), CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE );");
            migrationBuilder.Sql("IF OBJECT_ID('AspNetUserRoles', 'U') IS NULL CREATE TABLE [AspNetUserRoles] ( [UserId] nvarchar(450) NOT NULL, [RoleId] nvarchar(450) NOT NULL, CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]), CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE, CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE );");
            migrationBuilder.Sql("IF OBJECT_ID('AspNetUserTokens', 'U') IS NULL CREATE TABLE [AspNetUserTokens] ( [UserId] nvarchar(450) NOT NULL, [LoginProvider] nvarchar(450) NOT NULL, [Name] nvarchar(450) NOT NULL, [Value] nvarchar(max) NULL, CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]), CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE );");

            // Indexes (Safely add)
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CountryRisks_Name' AND object_id = OBJECT_ID('CountryRisks')) CREATE UNIQUE INDEX [IX_CountryRisks_Name] ON [CountryRisks] ([Name]);");


            // Notifications (Safe)
            migrationBuilder.Sql(@"IF OBJECT_ID('Notifications', 'U') IS NULL 
                CREATE TABLE [Notifications] ( 
                    [Id] int NOT NULL IDENTITY(1, 1), 
                    [UserId] nvarchar(450) NOT NULL, 
                    [Message] nvarchar(max) NOT NULL, 
                    [IsRead] bit NOT NULL, 
                    [CreatedAt] datetime2 NOT NULL, 
                    [Type] nvarchar(max) NOT NULL, 
                    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]), 
                    CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE 
                );");

            // Policies (Safe)
            migrationBuilder.Sql(@"IF OBJECT_ID('Policies', 'U') IS NULL 
                CREATE TABLE [Policies] ( 
                    [PolicyId] int NOT NULL IDENTITY(1, 1), 
                    [PolicyProductId] int NOT NULL, 
                    [PolicyNumber] nvarchar(max) NOT NULL, 
                    [CustomerId] nvarchar(max) NOT NULL, 
                    [AgentId] nvarchar(max) NULL, 
                    [Destination] nvarchar(max) NOT NULL, 
                    [PolicyType] nvarchar(max) NOT NULL, 
                    [PlanTier] nvarchar(max) NOT NULL, 
                    [TravellerName] nvarchar(max) NOT NULL, 
                    [TravellerAge] int NOT NULL, 
                    [PassportNumber] nvarchar(max) NOT NULL, 
                    [KycType] nvarchar(max) NOT NULL, 
                    [KycNumber] nvarchar(max) NOT NULL, 
                    [PremiumAmount] decimal(18,2) NOT NULL, 
                    [StartDate] datetime2 NOT NULL, 
                    [EndDate] datetime2 NOT NULL, 
                    [Status] int NOT NULL, 
                    [CreatedAt] datetime2 NOT NULL, 
                    CONSTRAINT [PK_Policies] PRIMARY KEY ([PolicyId]), 
                    CONSTRAINT [FK_Policies_PolicyProducts_PolicyProductId] FOREIGN KEY ([PolicyProductId]) REFERENCES [PolicyProducts] ([PolicyProductId]) ON DELETE CASCADE 
                );");

            // PolicyRequests (Safe)
            migrationBuilder.Sql(@"IF OBJECT_ID('PolicyRequests', 'U') IS NULL 
                CREATE TABLE [PolicyRequests] ( 
                    [PolicyRequestId] int NOT NULL IDENTITY(1, 1), 
                    [PolicyProductId] int NOT NULL, 
                    [CustomerId] nvarchar(450) NOT NULL, 
                    [AgentId] nvarchar(450) NULL, 
                    [Destination] nvarchar(max) NOT NULL, 
                    [StartDate] datetime2 NOT NULL, 
                    [EndDate] datetime2 NOT NULL, 
                    [TravellerName] nvarchar(max) NOT NULL, 
                    [TravellerAge] int NOT NULL, 
                    [PassportNumber] nvarchar(max) NOT NULL, 
                    [KycType] nvarchar(max) NOT NULL, 
                    [KycNumber] nvarchar(max) NOT NULL, 
                    [Dependents] nvarchar(max) NULL, 
                    [UniversityName] nvarchar(max) NULL, 
                    [StudentId] nvarchar(max) NULL, 
                    [TripFrequency] nvarchar(max) NULL, 
                    [RiskScore] int NOT NULL, 
                    [RiskAgeScore] int NOT NULL, 
                    [RiskDestinationScore] int NOT NULL, 
                    [RiskDurationScore] int NOT NULL, 
                    [RiskTierScore] int NOT NULL, 
                    [RiskLevel] nvarchar(max) NOT NULL, 
                    [Status] nvarchar(max) NOT NULL, 
                    [RejectionReason] nvarchar(max) NULL, 
                    [AgentNotes] nvarchar(max) NULL, 
                    [CalculatedPremium] decimal(18,2) NOT NULL, 
                    [RequestedAt] datetime2 NOT NULL, 
                    [ReviewedAt] datetime2 NULL, 
                    CONSTRAINT [PK_PolicyRequests] PRIMARY KEY ([PolicyRequestId]), 
                    CONSTRAINT [FK_PolicyRequests_AspNetUsers_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION, 
                    CONSTRAINT [FK_PolicyRequests_AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION, 
                    CONSTRAINT [FK_PolicyRequests_PolicyProducts_PolicyProductId] FOREIGN KEY ([PolicyProductId]) REFERENCES [PolicyProducts] ([PolicyProductId]) ON DELETE CASCADE 
                );");

            // Claims (Safe)
            migrationBuilder.Sql(@"IF OBJECT_ID('Claims', 'U') IS NULL 
                CREATE TABLE [Claims] ( 
                    [ClaimId] int NOT NULL IDENTITY(1, 1), 
                    [PolicyId] int NOT NULL, 
                    [CustomerId] nvarchar(max) NOT NULL, 
                    [ClaimType] nvarchar(max) NOT NULL, 
                    [Description] nvarchar(max) NOT NULL, 
                    [ClaimedAmount] decimal(18,2) NOT NULL, 
                    [ApprovedAmount] decimal(18,2) NULL, 
                    [Status] int NOT NULL, 
                    [ClaimsOfficerId] nvarchar(max) NULL, 
                    [RejectionReason] nvarchar(max) NULL, 
                    [SubmittedAt] datetime2 NOT NULL, 
                    [ReviewedAt] datetime2 NULL, 
                    [IncidentDate] datetime2 NOT NULL, 
                    [TravelSubtype] nvarchar(max) NULL, 
                    [AiSummary] nvarchar(max) NULL, 
                    CONSTRAINT [PK_Claims] PRIMARY KEY ([ClaimId]), 
                    CONSTRAINT [FK_Claims_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([PolicyId]) ON DELETE CASCADE 
                );");

            // Documents (Safe)
            migrationBuilder.Sql(@"IF OBJECT_ID('PolicyRequestDocuments', 'U') IS NULL 
                CREATE TABLE [PolicyRequestDocuments] ( 
                    [PolicyRequestDocumentId] int NOT NULL IDENTITY(1, 1), 
                    [PolicyRequestId] int NOT NULL, 
                    [FileName] nvarchar(max) NOT NULL, 
                    [FilePath] nvarchar(max) NOT NULL, 
                    [FileType] nvarchar(max) NOT NULL, 
                    [DocumentType] nvarchar(max) NOT NULL, 
                    [FileSize] bigint NOT NULL, 
                    [UploadedAt] datetime2 NOT NULL, 
                    CONSTRAINT [PK_PolicyRequestDocuments] PRIMARY KEY ([PolicyRequestDocumentId]), 
                    CONSTRAINT [FK_PolicyRequestDocuments_PolicyRequests_PolicyRequestId] FOREIGN KEY ([PolicyRequestId]) REFERENCES [PolicyRequests] ([PolicyRequestId]) ON DELETE CASCADE 
                );");

            migrationBuilder.Sql(@"IF OBJECT_ID('ClaimDocuments', 'U') IS NULL 
                CREATE TABLE [ClaimDocuments] ( 
                    [ClaimDocumentId] int NOT NULL IDENTITY(1, 1), 
                    [ClaimId] int NOT NULL, 
                    [FileName] nvarchar(max) NOT NULL, 
                    [FilePath] nvarchar(max) NOT NULL, 
                    [FileType] nvarchar(max) NOT NULL, 
                    [FileSize] bigint NOT NULL, 
                    [UploadedAt] datetime2 NOT NULL, 
                    CONSTRAINT [PK_ClaimDocuments] PRIMARY KEY ([ClaimDocumentId]), 
                    CONSTRAINT [FK_ClaimDocuments_Claims_ClaimId] FOREIGN KEY ([ClaimId]) REFERENCES [Claims] ([ClaimId]) ON DELETE CASCADE 
                );");

            // Indexes (Safe Existence Checks)
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_UserId' AND object_id = OBJECT_ID('Notifications')) CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Policies_PolicyProductId' AND object_id = OBJECT_ID('Policies')) CREATE INDEX [IX_Policies_PolicyProductId] ON [Policies] ([PolicyProductId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PolicyRequests_AgentId' AND object_id = OBJECT_ID('PolicyRequests')) CREATE INDEX [IX_PolicyRequests_AgentId] ON [PolicyRequests] ([AgentId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PolicyRequests_CustomerId' AND object_id = OBJECT_ID('PolicyRequests')) CREATE INDEX [IX_PolicyRequests_CustomerId] ON [PolicyRequests] ([CustomerId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PolicyRequests_PolicyProductId' AND object_id = OBJECT_ID('PolicyRequests')) CREATE INDEX [IX_PolicyRequests_PolicyProductId] ON [PolicyRequests] ([PolicyProductId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Claims_PolicyId' AND object_id = OBJECT_ID('Claims')) CREATE INDEX [IX_Claims_PolicyId] ON [Claims] ([PolicyId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClaimDocuments_ClaimId' AND object_id = OBJECT_ID('ClaimDocuments')) CREATE INDEX [IX_ClaimDocuments_ClaimId] ON [ClaimDocuments] ([ClaimId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PolicyRequestDocuments_PolicyRequestId' AND object_id = OBJECT_ID('PolicyRequestDocuments')) CREATE INDEX [IX_PolicyRequestDocuments_PolicyRequestId] ON [PolicyRequestDocuments] ([PolicyRequestId]);");

            // --- REPAIR: Ensure columns exist in existing tables (for dirty local DBs) ---
            migrationBuilder.Sql(@"
                IF OBJECT_ID('Claims', 'U') IS NOT NULL 
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'TravelSubtype')
                        ALTER TABLE [Claims] ADD [TravelSubtype] nvarchar(max) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'AiSummary')
                        ALTER TABLE [Claims] ADD [AiSummary] nvarchar(max) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('Policies', 'U') IS NOT NULL 
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Policies') AND name = 'PolicyType')
                        ALTER TABLE [Policies] ADD [PolicyType] nvarchar(max) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Policies') AND name = 'PlanTier')
                        ALTER TABLE [Policies] ADD [PlanTier] nvarchar(max) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ClaimDocuments");

            migrationBuilder.DropTable(
                name: "CountryRisks");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PolicyRequestDocuments");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "PolicyRequests");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "PolicyProducts");
        }
    }
}
