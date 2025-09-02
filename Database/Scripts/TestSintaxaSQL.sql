-- =============================================
-- SCRIPT TESTARE SINTAXA SQL - QUICK CHECK
-- Testeaz? doar sintaxa f?r? a executa queries complexe
-- =============================================

USE [ValyanMed];
GO

-- Test 1: Verific? existen?a tabelelor
PRINT '=== TEST 1: VERIFICARE TABELE ===';
IF OBJECT_ID('Departamente', 'U') IS NOT NULL
    PRINT '? Tabela Departamente exist?';
ELSE
    PRINT '? Tabela Departamente NU exist?';

IF OBJECT_ID('DepartamenteIerarhie', 'U') IS NOT NULL
    PRINT '? Tabela DepartamenteIerarhie exist?';
ELSE
    PRINT '? Tabela DepartamenteIerarhie NU exist?';

-- Test 2: Verific? sintaxa CTE simpl?
PRINT '';
PRINT '=== TEST 2: VERIFICARE SINTAXA CTE ===';
BEGIN TRY
    ;WITH TestCTE AS (
        SELECT 1 AS TestColumn
    )
    SELECT COUNT(*) AS TestResult FROM TestCTE;
    PRINT '? Sintaxa CTE este corect?';
END TRY
BEGIN CATCH
    PRINT '? Eroare sintaxa CTE: ' + ERROR_MESSAGE();
END CATCH

-- Test 3: Verific? sintaxa recursiv? CTE
PRINT '';
PRINT '=== TEST 3: VERIFICARE CTE RECURSIV ===';
BEGIN TRY
    ;WITH RecursivTest AS (
        -- Anchor - folosim CAST pentru a asigura tipul corect
        SELECT 1 AS Nivel, CAST('Test' AS NVARCHAR(100)) AS Nume
        
        UNION ALL
        
        -- Recursive part - acum tipurile se potrivesc
        SELECT 
            r.Nivel + 1, 
            CAST(r.Nume + '_' + CAST(r.Nivel + 1 AS NVARCHAR(10)) AS NVARCHAR(100))
        FROM RecursivTest r
        WHERE r.Nivel < 3
    )
    SELECT COUNT(*) AS TestRecursivResult FROM RecursivTest;
    PRINT '? Sintaxa CTE recursiv este corect?';
END TRY
BEGIN CATCH
    PRINT '? Eroare sintaxa CTE recursiv: ' + ERROR_MESSAGE();
END CATCH

-- Test 4: Verific? REPLICATE ?i CAST
PRINT '';
PRINT '=== TEST 4: VERIFICARE FUNCTII ===';
BEGIN TRY
    SELECT 
        REPLICATE('  ', 2) + 'Test' AS TestReplicate,
        CAST('Test' AS NVARCHAR(1000)) AS TestCast;
    PRINT '? Func?iile REPLICATE ?i CAST func?ioneaz? corect';
END TRY
BEGIN CATCH
    PRINT '? Eroare func?ii: ' + ERROR_MESSAGE();
END CATCH

-- Test 5: Verific? STRING_AGG (SQL Server 2017+)
PRINT '';
PRINT '=== TEST 5: VERIFICARE STRING_AGG ===';
BEGIN TRY
    SELECT STRING_AGG(name, ' > ') AS TestStringAgg
    FROM (
        SELECT 'Test1' AS name
        UNION ALL
        SELECT 'Test2'
        UNION ALL  
        SELECT 'Test3'
    ) t;
    PRINT '? Func?ia STRING_AGG func?ioneaz? corect';
END TRY
BEGIN CATCH
    PRINT '? Eroare STRING_AGG (SQL Server < 2017?): ' + ERROR_MESSAGE();
    PRINT '  Solu?ie: Înlocuie?te STRING_AGG cu FOR XML PATH';
    
    -- Alternative pentru versiuni mai vechi
    BEGIN TRY
        SELECT STUFF((
            SELECT ' > ' + name
            FROM (
                SELECT 'Test1' AS name
                UNION ALL
                SELECT 'Test2'
                UNION ALL  
                SELECT 'Test3'
            ) t2
            FOR XML PATH('')
        ), 1, 3, '') AS TestStringAggAlternative;
        PRINT '? Alternativa FOR XML PATH func?ioneaz?';
    END TRY
    BEGIN CATCH
        PRINT '? Eroare ?i cu alternativa FOR XML PATH: ' + ERROR_MESSAGE();
    END CATCH
END CATCH

-- Test 6: Verific? stored procedure syntax
PRINT '';
PRINT '=== TEST 6: VERIFICARE SP SYNTAX ===';
BEGIN TRY
    -- Test doar declarare procedure f?r? a o executa
    DECLARE @TestSQL NVARCHAR(MAX) = N'
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[sp_TestSyntax]'') AND type in (N''P'', N''PC''))
        DROP PROCEDURE [dbo].[sp_TestSyntax];
    
    CREATE PROCEDURE [dbo].[sp_TestSyntax]
        @TestParam NVARCHAR(200)
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT @TestParam AS TestOutput;
    END';
    
    EXEC sp_executesql @TestSQL;
    PRINT '? Sintaxa stored procedure este corect?';
END TRY
BEGIN CATCH
    PRINT '? Eroare sintaxa stored procedure: ' + ERROR_MESSAGE();
END CATCH

-- Test 7: Verific? tipurile de date pentru ierarhie
PRINT '';
PRINT '=== TEST 7: VERIFICARE TIPURI IERARHICE ===';
BEGIN TRY
    ;WITH TestIerarhie AS (
        -- Anchor - specific?m tipul explicit
        SELECT 
            1 AS ID,
            0 AS Nivel, 
            CAST('Root' AS NVARCHAR(200)) AS Nume,
            CAST('Root' AS NVARCHAR(1000)) AS Cale
        
        UNION ALL
        
        -- Recursive part - tipurile sunt acum consistente
        SELECT 
            t.ID + 1,
            t.Nivel + 1,
            CAST('Child_' + CAST(t.ID + 1 AS NVARCHAR(10)) AS NVARCHAR(200)),
            CAST(t.Cale + ' > Child_' + CAST(t.ID + 1 AS NVARCHAR(10)) AS NVARCHAR(1000))
        FROM TestIerarhie t
        WHERE t.Nivel < 2
    )
    SELECT COUNT(*) AS TestIerarhieResult FROM TestIerarhie;
    PRINT '? Tipurile ierarhice sunt corecte';
END TRY
BEGIN CATCH
    PRINT '? Eroare tipuri ierarhice: ' + ERROR_MESSAGE();
END CATCH

-- Test 8: Cleanup test procedure
BEGIN TRY
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TestSyntax]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[sp_TestSyntax];
END TRY
BEGIN CATCH
    -- Ignore cleanup errors
END CATCH

PRINT '';
PRINT '=======================================';
PRINT '       TESTARE SINTAXA COMPLET?       ';
PRINT '=======================================';

-- Afi?eaz? versiunea SQL Server pentru debugging
SELECT 
    @@VERSION AS [SQL Server Version],
    SERVERPROPERTY('ProductVersion') AS [Product Version],
    SERVERPROPERTY('ProductLevel') AS [Product Level],
    SERVERPROPERTY('CompatibilityLevel') AS [Compatibility Level];