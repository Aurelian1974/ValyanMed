-- =============================================
-- MASTER SETUP SCRIPT pentru Persoane
-- =============================================
-- Author: System
-- Create date: 2024
-- Description: Script principal pentru setup complet entitate Persoane
-- Ruleaza: Tabela + Stored Procedures + Date de test
-- =============================================

PRINT '================================================';
PRINT 'SETUP COMPLET PERSOANE - PORNIRE';
PRINT '================================================';

-- Step 1: Create table
PRINT '';
PRINT 'STEP 1: Crearea tabelei Persoane...';
PRINT '-----------------------------------';

-- Include table setup
EXEC('
-- Create table if not exists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name=''Persoane'' AND xtype=''U'')
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
        CONSTRAINT [UQ_Persoane_Guid] UNIQUE ([Guid]),
        CONSTRAINT [UQ_Persoane_CNP] UNIQUE ([CNP])
    );
    
    PRINT ''Tabela Persoane a fost creata cu succes.'';
END
ELSE
BEGIN
    PRINT ''Tabela Persoane exista deja.'';
END
');

-- Step 2: Create indexes
PRINT '';
PRINT 'STEP 2: Crearea indexurilor...';
PRINT '------------------------------';

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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Persoane') AND name = 'IX_Persoane_Judet_Localitate')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Persoane_Judet_Localitate] ON [dbo].[Persoane] ([Judet], [Localitate]);
    PRINT 'Index IX_Persoane_Judet_Localitate a fost creat.';
END

-- Step 3: Insert sample data
PRINT '';
PRINT 'STEP 3: Inserarea datelor de test...';
PRINT '-----------------------------------';

-- Check if sample data already exists
IF NOT EXISTS (SELECT * FROM Persoane WHERE Email LIKE '%@test.com')
BEGIN
    INSERT INTO Persoane (Nume, Prenume, Judet, Localitate, DataNasterii, Gen, Telefon, Email, CNP, EsteActiva)
    VALUES 
        ('Popescu', 'Ion', 'Bucuresti', 'Bucuresti', '1985-03-15', 'Masculin', '0721123456', 'ion.popescu@test.com', '1850315123456', 1),
        ('Ionescu', 'Maria', 'Cluj', 'Cluj-Napoca', '1990-07-22', 'Feminin', '0722234567', 'maria.ionescu@test.com', '2900722234567', 1),
        ('Georgescu', 'Andrei', 'Timis', 'Timisoara', '1978-11-08', 'Masculin', '0723345678', 'andrei.georgescu@test.com', '1781108345678', 1),
        ('Dumitrescu', 'Elena', 'Iasi', 'Iasi', '1992-02-14', 'Feminin', '0724456789', 'elena.dumitrescu@test.com', '2920214456789', 1),
        ('Marinescu', 'Cristian', 'Constanta', 'Constanta', '1987-09-30', 'Masculin', '0725567890', 'cristian.marinescu@test.com', '1870930567890', 0),
        ('Vasilescu', 'Ana', 'Brasov', 'Brasov', '1995-12-05', 'Feminin', '0726678901', 'ana.vasilescu@test.com', '2951205678901', 1),
        ('Stoica', 'Mihai', 'Dolj', 'Craiova', '1983-06-18', 'Masculin', '0727789012', 'mihai.stoica@test.com', '1830618789012', 1),
        ('Diaconu', 'Raluca', 'Galati', 'Galati', '1989-04-25', 'Feminin', '0728890123', 'raluca.diaconu@test.com', '2890425890123', 0),
        ('Florea', 'Adrian', 'Prahova', 'Ploiesti', '1991-01-12', 'Masculin', '0729901234', 'adrian.florea@test.com', '1910112901234', 1),
        ('Nicolae', 'Ioana', 'Arges', 'Pitesti', '1986-08-03', 'Feminin', '0720012345', 'ioana.nicolae@test.com', '2860803012345', 1);
    
    PRINT 'Date de test inserate cu succes: 10 persoane de test.';
END
ELSE
BEGIN
    PRINT 'Datele de test exista deja in tabela.';
END

PRINT '';
PRINT '================================================';
PRINT 'SETUP COMPLET PERSOANE - FINALIZAT CU SUCCES!';
PRINT '================================================';
PRINT '';
PRINT 'Verificari finale:';

-- Final verification
SELECT 
    'Persoane' as Tabela,
    COUNT(*) as NumarInregistrari,
    COUNT(CASE WHEN EsteActiva = 1 THEN 1 END) as Active,
    COUNT(CASE WHEN EsteActiva = 0 THEN 1 END) as Inactive
FROM Persoane;

PRINT '';
PRINT 'Setup complet! Aplicatia poate fi acum testata.';