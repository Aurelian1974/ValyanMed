-- =============================================
-- Author: System  
-- Create date: 2024
-- Description: Tabela Persoane pentru gestionarea persoanelor din sistem
-- =============================================

-- Create table if not exists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Persoane' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Persoane] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Guid] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [Nume] NVARCHAR(100) NOT NULL,
        [Prenume] NVARCHAR(100) NOT NULL,
        [Judet] NVARCHAR(50) NULL,
        [Localitate] NVARCHAR(100) NULL,
        [Strada] NVARCHAR(100) NULL,
        [NumarStrada] NVARCHAR(10) NULL,
        [CodPostal] NVARCHAR(10) NULL,
        [PozitieOrganizatie] NVARCHAR(100) NULL,
        [DataNasterii] DATE NULL,
        [CNP] NVARCHAR(13) NULL,
        [TipActIdentitate] NVARCHAR(20) NULL,
        [SerieActIdentitate] NVARCHAR(10) NULL,
        [NumarActIdentitate] NVARCHAR(20) NULL,
        [StareCivila] NVARCHAR(20) NULL,
        [Gen] NVARCHAR(20) NULL,
        [Telefon] NVARCHAR(20) NULL,
        [Email] NVARCHAR(100) NULL,
        [EsteActiva] BIT NOT NULL DEFAULT 1,
        [DataCreare] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        [DataModificare] DATETIME2(7) NULL,
        
        CONSTRAINT [PK_Persoane] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Persoane_Guid] UNIQUE ([Guid])
    );
    
    -- Create filtered unique index for CNP (allows multiple NULLs)
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_Persoane_CNP] 
    ON [dbo].[Persoane] ([CNP])
    WHERE [CNP] IS NOT NULL;
    
    PRINT 'Tabela Persoane a fost creata cu succes.';
END
ELSE
BEGIN
    PRINT 'Tabela Persoane exista deja.';
    
    -- Check if the old UNIQUE constraint exists and drop it
    IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'UQ_Persoane_CNP' AND parent_object_id = OBJECT_ID('Persoane'))
    BEGIN
        ALTER TABLE Persoane DROP CONSTRAINT [UQ_Persoane_CNP];
        PRINT 'Constraint vechi UQ_Persoane_CNP a fost eliminat.';
    END
    
    -- Create the filtered unique index if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Persoane') AND name = 'UQ_Persoane_CNP')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_Persoane_CNP] 
        ON [dbo].[Persoane] ([CNP])
        WHERE [CNP] IS NOT NULL;
        PRINT 'Index unic filtrat UQ_Persoane_CNP a fost creat.';
    END
    
    -- Add missing columns if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Persoane') AND name = 'Telefon')
    BEGIN
        ALTER TABLE Persoane ADD [Telefon] NVARCHAR(20) NULL;
        PRINT 'Coloana Telefon a fost adaugata.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Persoane') AND name = 'Email')
    BEGIN
        ALTER TABLE Persoane ADD [Email] NVARCHAR(100) NULL;
        PRINT 'Coloana Email a fost adaugata.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Persoane') AND name = 'EsteActiva')
    BEGIN
        ALTER TABLE Persoane ADD [EsteActiva] BIT NOT NULL DEFAULT 1;
        PRINT 'Coloana EsteActiva a fost adaugata.';
    END
END

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Persoane') AND name = 'IX_Persoane_Nume_Prenume')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Persoane_Nume_Prenume] ON [dbo].[Persoane] ([Nume], [Prenume]);
    PRINT 'Index IX_Persoane_Nume_Prenume a fost creat.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Persoane') AND name = 'IX_Persoane_CNP')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Persoane_CNP] ON [dbo].[Persoane] ([CNP]) WHERE [CNP] IS NOT NULL;
    PRINT 'Index IX_Persoane_CNP a fost creat.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Persoane') AND name = 'IX_Persoane_Email')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Persoane_Email] ON [dbo].[Persoane] ([Email]) WHERE [Email] IS NOT NULL;
    PRINT 'Index IX_Persoane_Email a fost creat.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Persoane') AND name = 'IX_Persoane_Telefon')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Persoane_Telefon] ON [dbo].[Persoane] ([Telefon]) WHERE [Telefon] IS NOT NULL;
    PRINT 'Index IX_Persoane_Telefon a fost creat.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Persoane') AND name = 'IX_Persoane_Judet_Localitate')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Persoane_Judet_Localitate] ON [dbo].[Persoane] ([Judet], [Localitate]);
    PRINT 'Index IX_Persoane_Judet_Localitate a fost creat.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Persoane') AND name = 'IX_Persoane_EsteActiva')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Persoane_EsteActiva] ON [dbo].[Persoane] ([EsteActiva]);
    PRINT 'Index IX_Persoane_EsteActiva a fost creat.';
END

PRINT 'Setup complet pentru tabela Persoane finalizat cu succes!';