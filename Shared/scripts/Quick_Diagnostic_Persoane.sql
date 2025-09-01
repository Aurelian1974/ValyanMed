-- =============================================
-- QUICK FIX - VERIFICA SI REPAREAZ? PROBLEMA PERSOANE
-- =============================================

PRINT '=== QUICK DIAGNOSTIC PENTRU PERSOANE ===';

-- 1. Verific? dac? tabela exist?
IF EXISTS (SELECT * FROM sysobjects WHERE name='Persoane' AND xtype='U')
BEGIN
    PRINT '? Tabela Persoane exista';
    
    -- Check data
    DECLARE @Count INT;
    SELECT @Count = COUNT(*) FROM Persoane;
    PRINT CONCAT('? Numar inregistrari: ', @Count);
    
    -- Check columns
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Persoane') AND name = 'EsteActiva')
        PRINT '? Coloana EsteActiva exista';
    ELSE
        PRINT '? Coloana EsteActiva NU exista';
        
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Persoane') AND name = 'Telefon')
        PRINT '? Coloana Telefon exista';
    ELSE
        PRINT '? Coloana Telefon NU exista';
END
ELSE
BEGIN
    PRINT '? Tabela Persoane NU exista!';
END

-- 2. Verific? stored procedures
IF OBJECT_ID('sp_GetPersoanePagedWithSearch', 'P') IS NOT NULL
    PRINT '? sp_GetPersoanePagedWithSearch exista';
ELSE
    PRINT '? sp_GetPersoanePagedWithSearch NU exista!';

IF OBJECT_ID('sp_CheckCNPExists', 'P') IS NOT NULL
    PRINT '? sp_CheckCNPExists exista';
ELSE
    PRINT '? sp_CheckCNPExists NU exista!';

-- 3. Test rapid stored procedure
IF OBJECT_ID('sp_GetPersoanePagedWithSearch', 'P') IS NOT NULL
BEGIN
    PRINT '';
    PRINT '=== TEST STORED PROCEDURE ===';
    
    BEGIN TRY
        EXEC sp_GetPersoanePagedWithSearch 
            @Search = NULL,
            @Judet = NULL, 
            @Localitate = NULL,
            @EsteActiva = NULL,
            @Page = 1,
            @PageSize = 5,
            @Sort = NULL;
        
        PRINT '? sp_GetPersoanePagedWithSearch functioneaza OK';
    END TRY
    BEGIN CATCH
        PRINT '? EROARE in sp_GetPersoanePagedWithSearch:';
        PRINT CONCAT('   Error: ', ERROR_MESSAGE());
        PRINT CONCAT('   Line: ', ERROR_LINE());
    END CATCH
END

PRINT '';
PRINT '=== DIAGNOSTIC COMPLET ===';

-- Afi?eaz? primele 3 înregistr?ri pentru test
IF EXISTS (SELECT * FROM Persoane)
BEGIN
    PRINT 'Primele 3 persoane din baza de date:';
    SELECT TOP 3 
        Id, 
        CONCAT(Nume, ' ', Prenume) as NumeComplet,
        Email,
        Telefon,
        EsteActiva
    FROM Persoane
    ORDER BY Id;
END
ELSE
BEGIN
    PRINT 'Nu sunt date in tabela Persoane.';
END