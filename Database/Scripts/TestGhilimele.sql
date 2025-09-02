-- =============================================
-- SCRIPT VERIFICARE RAPIDA GHILIMELE SI SINTAXA
-- Verific? probleme comune de sintax? ?i ghilimele
-- =============================================

USE [ValyanMed];
GO

PRINT '=== VERIFICARE RAPIDA SINTAXA SI GHILIMELE ===';
PRINT '';

-- Test 1: Ghilimele simple
PRINT '=== TEST 1: GHILIMELE SIMPLE ===';
BEGIN TRY
    DECLARE @TestString1 NVARCHAR(100) = N'Test simplu cu ghilimele';
    PRINT '? Ghilimele simple func?ioneaz?: ' + @TestString1;
END TRY
BEGIN CATCH
    PRINT '? Eroare ghilimele simple: ' + ERROR_MESSAGE();
END CATCH

-- Test 2: Apostrof Ón string
PRINT '';
PRINT '=== TEST 2: APOSTROF IN STRING ===';
BEGIN TRY
    DECLARE @TestString2 NVARCHAR(100) = N'Test cu apostrof: Don''t worry';
    PRINT '? Apostrof Ón string func?ioneaz?: ' + @TestString2;
END TRY
BEGIN CATCH
    PRINT '? Eroare apostrof Ón string: ' + ERROR_MESSAGE();
END CATCH

-- Test 3: Ghilimele nested Ón PRINT
PRINT '';
PRINT '=== TEST 3: GHILIMELE NESTED ===';
BEGIN TRY
    PRINT 'Exemplu EXEC: sp_Test @param = N''value'';';
    PRINT '? Ghilimele nested Ón PRINT func?ioneaz?';
END TRY
BEGIN CATCH
    PRINT '? Eroare ghilimele nested: ' + ERROR_MESSAGE();
END CATCH

-- Test 4: Unicode strings
PRINT '';
PRINT '=== TEST 4: UNICODE STRINGS ===';
BEGIN TRY
    DECLARE @TestUnicode NVARCHAR(100) = N'Test Unicode: ‚Ó???';
    PRINT '? Unicode strings func?ioneaz?: ' + @TestUnicode;
END TRY
BEGIN CATCH
    PRINT '? Eroare Unicode strings: ' + ERROR_MESSAGE();
END CATCH

-- Test 5: Nume cu spatii (square brackets)
PRINT '';
PRINT '=== TEST 5: NUME CU SPATII ===';
BEGIN TRY
    -- Simul?m o tabel? cu nume cu spa?ii
    IF OBJECT_ID(N'[Test Tabel Cu Spatii]', 'U') IS NOT NULL
        DROP TABLE [Test Tabel Cu Spatii];
    
    CREATE TABLE [Test Tabel Cu Spatii] (
        [Coloana Cu Spatii] INT,
        [Alta Coloana] NVARCHAR(50)
    );
    
    INSERT INTO [Test Tabel Cu Spatii] ([Coloana Cu Spatii], [Alta Coloana])
    VALUES (1, N'Test');
    
    SELECT COUNT(*) AS TestCount FROM [Test Tabel Cu Spatii];
    
    DROP TABLE [Test Tabel Cu Spatii];
    
    PRINT '? Nume cu spa?ii (square brackets) func?ioneaz?';
END TRY
BEGIN CATCH
    PRINT '? Eroare nume cu spa?ii: ' + ERROR_MESSAGE();
END CATCH

-- Test 6: Stored procedure cu parametri string
PRINT '';
PRINT '=== TEST 6: STORED PROCEDURE CU PARAMETRI ===';
BEGIN TRY
    -- Creeaz? SP de test
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TestGhilimele]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[sp_TestGhilimele];
    
    EXEC(N'
    CREATE PROCEDURE [dbo].[sp_TestGhilimele]
        @Input NVARCHAR(200)
    AS
    BEGIN
        SET NOCOUNT ON;
        PRINT ''Input primit: '' + @Input;
        SELECT @Input AS [Parametru Primit];
    END');
    
    -- Testeaz? SP cu ghilimele
    EXEC sp_TestGhilimele @Input = N'Test cu ''apostrof'' ?i ghilimele';
    
    -- Cleanup
    DROP PROCEDURE [dbo].[sp_TestGhilimele];
    
    PRINT '? Stored procedure cu parametri string func?ioneaz?';
END TRY
BEGIN CATCH
    PRINT '? Eroare stored procedure parametri: ' + ERROR_MESSAGE();
END CATCH

-- Test 7: Dynamic SQL cu ghilimele
PRINT '';
PRINT '=== TEST 7: DYNAMIC SQL ===';
BEGIN TRY
    DECLARE @DynamicSQL NVARCHAR(MAX);
    DECLARE @TestParam NVARCHAR(100) = N'Test ''dynamic'' SQL';
    
    SET @DynamicSQL = N'SELECT ''' + REPLACE(@TestParam, '''', '''''') + ''' AS [Dynamic Result]';
    
    EXEC sp_executesql @DynamicSQL;
    
    PRINT '? Dynamic SQL cu ghilimele func?ioneaz?';
END TRY
BEGIN CATCH
    PRINT '? Eroare Dynamic SQL: ' + ERROR_MESSAGE();
END CATCH

-- Test 8: PRINT cu EXEC examples (cum sunt Ón stored procedures)
PRINT '';
PRINT '=== TEST 8: PRINT CU EXEC EXAMPLES ===';
BEGIN TRY
    -- Simuleaz? exemplele din StoredProceduresDepartamente.sql
    PRINT 'Exemple de utilizare:';
    PRINT '';
    PRINT '-- Descendentii categoriei Medicale:';
    PRINT 'EXEC sp_GetDescendenti @NumeDepartament = N''Medicale'', @TipDepartament = N''Categorie'';';
    PRINT '';
    PRINT '-- Export structura completa:';
    PRINT 'EXEC sp_ExportaStructura @FormatOutput = N''TREE'';';
    PRINT '';
    PRINT '-- Cautare cu pattern:';
    PRINT 'EXEC sp_CautaDepartamente @SearchPattern = N''cardio'';';
    
    PRINT '? PRINT cu EXEC examples func?ioneaz? corect';
END TRY
BEGIN CATCH
    PRINT '? Eroare PRINT cu EXEC: ' + ERROR_MESSAGE();
END CATCH

-- Test 9: FOR XML PATH alternative pentru STRING_AGG
PRINT '';
PRINT '=== TEST 9: FOR XML PATH ALTERNATIVE ===';
BEGIN TRY
    ;WITH TestData AS (
        SELECT 'Categoria1' AS Nume
        UNION ALL
        SELECT 'Categoria2'
        UNION ALL
        SELECT 'Categoria3'
    )
    SELECT STUFF((
        SELECT ' > ' + Nume
        FROM TestData
        FOR XML PATH('')
    ), 1, 3, '') AS [Concatenated Result];
    
    PRINT '? FOR XML PATH alternative func?ioneaz?';
END TRY
BEGIN CATCH
    PRINT '? Eroare FOR XML PATH: ' + ERROR_MESSAGE();
END CATCH

-- Test 10: Verificare caractere speciale
PRINT '';
PRINT '=== TEST 10: CARACTERE SPECIALE ===';
BEGIN TRY
    DECLARE @SpecialChars NVARCHAR(200) = N'Test cu: ‡·‚„‰ÂÊÁËÈÍÎ & %_[]{}()';
    SELECT @SpecialChars AS [Special Characters Test];
    PRINT '? Caractere speciale func?ioneaz?: ' + @SpecialChars;
END TRY
BEGIN CATCH
    PRINT '? Eroare caractere speciale: ' + ERROR_MESSAGE();
END CATCH

PRINT '';
PRINT '=================================================';
PRINT '        VERIFICARE GHILIMELE COMPLET?';
PRINT '=================================================';
PRINT '';
PRINT 'Dac? toate testele au trecut, scripturile SQL';
PRINT 'ar trebui s? ruleze f?r? probleme de ghilimele.';
PRINT '';
PRINT 'Pentru debugging avansat:';
PRINT '1. Folose?te SSMS cu syntax highlighting';
PRINT '2. Activeaz? "Show whitespace characters"';
PRINT '3. Verific? encoding-ul fi?ierelor (UTF-8 vs ANSI)';
PRINT '4. Testeaz? sec?iuni mici de cod individual';
PRINT '=================================================';