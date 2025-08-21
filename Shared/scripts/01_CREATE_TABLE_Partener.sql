/*
    Create Table: dbo.Partener
    Purpose: Store partner information with similar structure to Judet table
    
    Columns:
    - PartenerId: Primary key, auto-increment
    - PartenerGuid: Unique identifier with sequential generation
    - CodIntern: Internal code for the partner
    - Denumire: Partner name/denomination
    - CodFiscal: Tax identification code
    - Judet: County/region
    - Localitate: City/locality
    - Adresa: Address
    - DataCreare: Creation timestamp
    - DataActualizare: Last update timestamp
    - UtilizatorCreare: User who created the record
    - UtilizatorActualizare: User who last updated the record
    - Activ: Active status flag
*/

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
    
    PRINT 'Table dbo.Partener created successfully with indexes.';
END
ELSE
BEGIN
    PRINT 'Table dbo.Partener already exists.';
END
GO