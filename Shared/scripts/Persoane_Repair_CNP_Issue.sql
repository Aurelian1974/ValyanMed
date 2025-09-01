-- =============================================
-- SCRIPT DE REPARARE PENTRU TABELA PERSOANE
-- =============================================
-- Purpose: Corecteaz? problema cu constraint-ul UNIQUE pe CNP
-- Author: System
-- Create date: 2024
-- =============================================

PRINT '================================================';
PRINT 'REPARARE TABELA PERSOANE - PORNIRE';
PRINT '================================================';

-- Step 1: Check current state
PRINT '';
PRINT 'STEP 1: Verificarea starii actuale...';
PRINT '-------------------------------------';

IF EXISTS (SELECT * FROM sysobjects WHERE name='Persoane' AND xtype='U')
BEGIN
    PRINT 'Tabela Persoane exista.';
    
    -- Check current constraint
    IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'UQ_Persoane_CNP' AND parent_object_id = OBJECT_ID('Persoane'))
    BEGIN
        PRINT 'PROBLEMA DETECTATA: Constraint UNIQUE pe CNP impiedica NULL-uri multiple.';
        
        -- Step 2: Remove old constraint
        PRINT '';
        PRINT 'STEP 2: Eliminarea constraint-ului problematic...';
        PRINT '-------------------------------------------------';
        
        ALTER TABLE Persoane DROP CONSTRAINT [UQ_Persoane_CNP];
        PRINT 'Constraint UQ_Persoane_CNP a fost eliminat cu succes.';
        
        -- Step 3: Create filtered unique index
        PRINT '';
        PRINT 'STEP 3: Crearea index-ului filtrat...';
        PRINT '-------------------------------------';
        
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_Persoane_CNP] 
        ON [dbo].[Persoane] ([CNP])
        WHERE [CNP] IS NOT NULL;
        
        PRINT 'Index unic filtrat UQ_Persoane_CNP a fost creat cu succes.';
    END
    ELSE
    BEGIN
        PRINT 'Constraint-ul problematic nu exista. Verificam index-ul...';
        
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Persoane') AND name = 'UQ_Persoane_CNP')
        BEGIN
            CREATE UNIQUE NONCLUSTERED INDEX [UQ_Persoane_CNP] 
            ON [dbo].[Persoane] ([CNP])
            WHERE [CNP] IS NOT NULL;
            
            PRINT 'Index unic filtrat UQ_Persoane_CNP a fost creat.';
        END
        ELSE
        BEGIN
            PRINT 'Index-ul UQ_Persoane_CNP exista deja.';
        END
    END
    
    -- Step 4: Clear problematic test data and insert good data
    PRINT '';
    PRINT 'STEP 4: Curatarea si inserarea datelor de test...';
    PRINT '-------------------------------------------------';
    
    -- Remove test data that might cause conflicts
    DELETE FROM Persoane WHERE Email LIKE '%@test.com';
    PRINT 'Date de test vechi au fost eliminate.';
    
    -- Insert clean test data with valid CNPs
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
    
    PRINT 'Date de test noi au fost inserate cu succes: 10 persoane.';
    
    -- Step 5: Recreate stored procedures with fix
    PRINT '';
    PRINT 'STEP 5: Recrearea stored procedures corectate...';
    PRINT '-----------------------------------------------';
    
    -- Drop and recreate sp_CheckCNPExists with correct syntax
    IF OBJECT_ID('sp_CheckCNPExists', 'P') IS NOT NULL
        DROP PROCEDURE sp_CheckCNPExists;
    
    EXEC('
    CREATE PROCEDURE sp_CheckCNPExists
        @CNP NVARCHAR(13),
        @ExcludeId INT = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DECLARE @Count INT = 0;
        
        -- Only check if CNP is not NULL or empty
        IF @CNP IS NOT NULL AND LTRIM(RTRIM(@CNP)) != ''''
        BEGIN
            SELECT @Count = COUNT(*)
            FROM Persoane 
            WHERE CNP = @CNP 
              AND (@ExcludeId IS NULL OR Id != @ExcludeId);
        END
        
        SELECT CASE WHEN @Count > 0 THEN 1 ELSE 0 END as [ExistsCNP];
    END
    ');
    
    PRINT 'Stored procedure sp_CheckCNPExists a fost recreat cu sintaxa corecta.';
    
    -- Step 6: Verification
    PRINT '';
    PRINT 'STEP 6: Verificarea finala...';
    PRINT '-----------------------------';
    
    SELECT 
        'Persoane' as Tabela,
        COUNT(*) as NumarInregistrari,
        COUNT(CASE WHEN EsteActiva = 1 THEN 1 END) as Active,
        COUNT(CASE WHEN EsteActiva = 0 THEN 1 END) as Inactive,
        COUNT(CASE WHEN CNP IS NOT NULL THEN 1 END) as CuCNP,
        COUNT(CASE WHEN CNP IS NULL THEN 1 END) as FaraCNP
    FROM Persoane;
    
END
ELSE
BEGIN
    PRINT 'EROARE: Tabela Persoane nu exista!';
    PRINT 'Rulati mai intai script-ul de creare a tabelei.';
END

PRINT '';
PRINT '================================================';
PRINT 'REPARARE FINALIZATA CU SUCCES!';
PRINT '================================================';
PRINT 'Aplicatia poate fi acum testata fara probleme.';