/*
    Master Script: Setup Partener table and stored procedures
    Purpose: Execute all scripts in correct order to create Partener functionality
    
    Usage: 
    1. Open SQL Server Management Studio
    2. Connect to your database: DESKTOP-3Q8HI82\ERP, Database: ValyanMed
    3. Execute this script
    
    Or execute individual scripts in this order:
    1. 01_CREATE_TABLE_Partener.sql
    2. 02_sp_Partener_GetAll.sql
    3. 03_sp_Partener_GetById.sql
    4. 04_sp_Partener_Create.sql
    5. 05_sp_Partener_Update.sql
    6. 06_sp_Partener_Delete.sql
    7. 07_sp_Partener_GetPaged.sql
*/

USE [ValyanMed]
GO

PRINT '=== Starting Partener table and procedures setup ==='
PRINT 'Database: ' + DB_NAME()
PRINT 'Server: ' + @@SERVERNAME
PRINT 'Start time: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT ''

-- 1. Create Table
PRINT '1. Creating table dbo.Partener...'
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Partener' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Partener] (
        [PartenerId]           INT              IDENTITY (1, 1) NOT NULL,
        [PartenerGuid]         UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
        [CodIntern]            NVARCHAR (50)    NOT NULL,
        [Denumire]             NVARCHAR (200)   NOT NULL,
        [CodFiscal]            NVARCHAR (50)    NULL,
        [Judet]                NVARCHAR (100)   NULL,
        [Localitate]           NVARCHAR (100)   NULL,
        [Adresa]               NVARCHAR (500)   NULL,
        [DataCreare]           DATETIME2        DEFAULT (GETDATE()) NOT NULL,
        [DataActualizare]      DATETIME2        DEFAULT (GETDATE()) NOT NULL,
        [UtilizatorCreare]     NVARCHAR (100)   NULL,
        [UtilizatorActualizare] NVARCHAR (100)  NULL,
        [Activ]                BIT              DEFAULT (1) NOT NULL,
        
        CONSTRAINT [PK_Partener] PRIMARY KEY CLUSTERED ([PartenerId] ASC),
        CONSTRAINT [UQ_Partener_Guid] UNIQUE ([PartenerGuid]),
        CONSTRAINT [UQ_Partener_CodIntern] UNIQUE ([CodIntern])
    );
    
    -- Create indexes for better performance
    CREATE NONCLUSTERED INDEX [IX_Partener_CodFiscal] ON [dbo].[Partener] ([CodFiscal]);
    CREATE NONCLUSTERED INDEX [IX_Partener_Denumire] ON [dbo].[Partener] ([Denumire]);
    CREATE NONCLUSTERED INDEX [IX_Partener_Judet] ON [dbo].[Partener] ([Judet]);
    CREATE NONCLUSTERED INDEX [IX_Partener_Activ] ON [dbo].[Partener] ([Activ]);
    
    PRINT '   ? Table dbo.Partener created successfully with indexes.';
END
ELSE
BEGIN
    PRINT '   ? Table dbo.Partener already exists.';
END

-- 2. Create stored procedures
PRINT ''
PRINT '2. Creating stored procedures...'

-- sp_Partener_GetAll
PRINT '   Creating sp_Partener_GetAll...'
IF OBJECT_ID('dbo.sp_Partener_GetAll','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Partener_GetAll AS BEGIN SET NOCOUNT ON; END');

EXEC('
ALTER PROCEDURE dbo.sp_Partener_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PartenerId, PartenerGuid, CodIntern, Denumire, CodFiscal, Judet, Localitate, Adresa,
        DataCreare, DataActualizare, UtilizatorCreare, UtilizatorActualizare, Activ
    FROM dbo.Partener WITH (NOLOCK)
    WHERE Activ = 1
    ORDER BY Denumire ASC;
END');
PRINT '   ? sp_Partener_GetAll created/updated'

-- sp_Partener_GetById
PRINT '   Creating sp_Partener_GetById...'
IF OBJECT_ID('dbo.sp_Partener_GetById','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Partener_GetById AS BEGIN SET NOCOUNT ON; END');

EXEC('
ALTER PROCEDURE dbo.sp_Partener_GetById
    @PartenerId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PartenerId, PartenerGuid, CodIntern, Denumire, CodFiscal, Judet, Localitate, Adresa,
        DataCreare, DataActualizare, UtilizatorCreare, UtilizatorActualizare, Activ
    FROM dbo.Partener WITH (NOLOCK)
    WHERE PartenerId = @PartenerId;
END');
PRINT '   ? sp_Partener_GetById created/updated'

-- Test the setup
PRINT ''
PRINT '3. Testing setup...'

BEGIN TRY
    -- Test table exists
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Partener' AND schema_id = SCHEMA_ID('dbo'))
        PRINT '   ? Table dbo.Partener exists'
    ELSE
        PRINT '   ? Table dbo.Partener missing'

    -- Test procedures exist
    IF OBJECT_ID('dbo.sp_Partener_GetAll','P') IS NOT NULL
        PRINT '   ? sp_Partener_GetAll exists'
    ELSE
        PRINT '   ? sp_Partener_GetAll missing'

    IF OBJECT_ID('dbo.sp_Partener_GetById','P') IS NOT NULL
        PRINT '   ? sp_Partener_GetById exists'
    ELSE
        PRINT '   ? sp_Partener_GetById missing'

    -- Test basic functionality
    EXEC dbo.sp_Partener_GetAll;
    PRINT '   ? sp_Partener_GetAll executed successfully'

END TRY
BEGIN CATCH
    PRINT '   ? Error during testing: ' + ERROR_MESSAGE()
END CATCH

PRINT ''
PRINT '=== Setup completed ==='
PRINT 'End time: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT ''
PRINT 'Next steps:'
PRINT '1. Execute remaining stored procedures (Create, Update, Delete, GetPaged)'
PRINT '2. Add sample data if needed'
PRINT '3. Create C# DTOs and repository classes'
PRINT '4. Create API controller'
PRINT '5. Create Blazor components'