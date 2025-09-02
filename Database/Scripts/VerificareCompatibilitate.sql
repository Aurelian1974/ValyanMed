-- =============================================
-- SCRIPT VERIFICARE COMPATIBILITATE SQL SERVER
-- Verific? toate func?ionalit??ile necesare pentru departamente ierarhice
-- =============================================

USE [ValyanMed];
GO

DECLARE @ErrorCount INT = 0;
DECLARE @WarningCount INT = 0;

PRINT '================================================';
PRINT '  VERIFICARE COMPATIBILITATE SQL SERVER COMPLET';
PRINT '================================================';
PRINT '';

-- =============================================
-- 1. INFORMA?II SISTEM
-- =============================================
PRINT '=== 1. INFORMA?II SISTEM ===';

DECLARE @Version NVARCHAR(255) = CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(255));
DECLARE @VersionMajor INT = CAST(SUBSTRING(@Version, 1, CHARINDEX('.', @Version) - 1) AS INT);
DECLARE @Edition NVARCHAR(255) = CAST(SERVERPROPERTY('Edition') AS NVARCHAR(255));
DECLARE @Level NVARCHAR(255) = CAST(SERVERPROPERTY('ProductLevel') AS NVARCHAR(255));

PRINT '? SQL Server Version: ' + @Version;
PRINT '? Edition: ' + @Edition;
PRINT '? Service Pack Level: ' + @Level;
PRINT '? Version Major: ' + CAST(@VersionMajor AS NVARCHAR(10));

IF @VersionMajor >= 14  -- SQL Server 2017+
    PRINT '? STRING_AGG suportat (SQL Server 2017+)';
ELSE
BEGIN
    PRINT '? STRING_AGG NU este suportat - va folosi FOR XML PATH';
    SET @WarningCount = @WarningCount + 1;
END

PRINT '';

-- =============================================
-- 2. VERIFICARE TABELE NECESARE
-- =============================================
PRINT '=== 2. VERIFICARE TABELE ===';

IF OBJECT_ID('Departamente', 'U') IS NOT NULL
    PRINT '? Tabela Departamente exist?';
ELSE
BEGIN
    PRINT '? Tabela Departamente NU exist? - trebuie creat?';
    SET @ErrorCount = @ErrorCount + 1;
END

IF OBJECT_ID('DepartamenteIerarhie', 'U') IS NOT NULL
    PRINT '? Tabela DepartamenteIerarhie exist?';
ELSE
BEGIN
    PRINT '? Tabela DepartamenteIerarhie NU exist? - trebuie creat?';
    SET @ErrorCount = @ErrorCount + 1;
END

PRINT '';

-- =============================================
-- 3. TEST TIPURI DE DATE
-- =============================================
PRINT '=== 3. TEST TIPURI DE DATE ===';

BEGIN TRY
    DECLARE @TestTable TABLE (
        ID UNIQUEIDENTIFIER DEFAULT NEWID(),
        Nume NVARCHAR(200),
        Tip NVARCHAR(50)
    );
    
    INSERT INTO @TestTable (Nume, Tip)
    VALUES 
        (N'Test1', N'Categorie'),
        (N'Test2', N'Specialitate');
    
    IF (SELECT COUNT(*) FROM @TestTable) = 2
        PRINT '? Tipurile de date UNIQUEIDENTIFIER ?i NVARCHAR func?ioneaz? corect';
    ELSE
    BEGIN
        PRINT '? Probleme cu tipurile de date de baz?';
        SET @ErrorCount = @ErrorCount + 1;
    END
END TRY
BEGIN CATCH
    PRINT '? Eroare tipuri de date: ' + ERROR_MESSAGE();
    SET @ErrorCount = @ErrorCount + 1;
END CATCH

PRINT '';

-- =============================================
-- 4. TEST CTE SIMPLE
-- =============================================
PRINT '=== 4. TEST CTE SIMPLE ===';

BEGIN TRY
    ;WITH SimpleCTE AS (
        SELECT 
            CAST('Test' AS NVARCHAR(100)) AS Nume,
            1 AS Nivel
    )
    SELECT COUNT(*) AS TestResult 
    FROM SimpleCTE;
    
    PRINT '? CTE simple func?ioneaz? corect';
END TRY
BEGIN CATCH
    PRINT '? Eroare CTE simple: ' + ERROR_MESSAGE();
    SET @ErrorCount = @ErrorCount + 1;
END CATCH

PRINT '';

-- =============================================
-- 5. TEST CTE RECURSIV CU TIPURI CORECTE
-- =============================================
PRINT '=== 5. TEST CTE RECURSIV ===';

BEGIN TRY
    ;WITH RecursiveCTE AS (
        -- Anchor: specific?m tipurile explicit
        SELECT 
            1 AS Nivel,
            CAST('Root' AS NVARCHAR(200)) AS Nume,
            CAST('Root' AS NVARCHAR(1000)) AS Cale,
            NEWID() AS ID
        
        UNION ALL
        
        -- Recursive part: acelea?i tipuri
        SELECT 
            r.Nivel + 1,
            CAST('Child_' + CAST(r.Nivel + 1 AS NVARCHAR(10)) AS NVARCHAR(200)),
            CAST(r.Cale + ' > Child_' + CAST(r.Nivel + 1 AS NVARCHAR(10)) AS NVARCHAR(1000)),
            NEWID()
        FROM RecursiveCTE r
        WHERE r.Nivel < 3
    )
    SELECT COUNT(*) AS RecursiveTestResult 
    FROM RecursiveCTE;
    
    PRINT '? CTE recursiv cu tipuri corecte func?ioneaz?';
END TRY
BEGIN CATCH
    PRINT '? Eroare CTE recursiv: ' + ERROR_MESSAGE();
    SET @ErrorCount = @ErrorCount + 1;
END CATCH

PRINT '';

-- =============================================
-- 6. TEST FUNC?II DE STRING
-- =============================================
PRINT '=== 6. TEST FUNC?II DE STRING ===';

BEGIN TRY
    SELECT 
        REPLICATE('  ', 3) + 'Test' AS TestReplicate,
        CAST('TestCast' AS NVARCHAR(1000)) AS TestCast,
        UPPER('test') AS TestUpper,
        CONCAT('A', 'B', 'C') AS TestConcat;
    
    PRINT '? Func?iile de string (REPLICATE, CAST, UPPER, CONCAT) func?ioneaz?';
END TRY
BEGIN CATCH
    PRINT '? Eroare func?ii string: ' + ERROR_MESSAGE();
    SET @ErrorCount = @ErrorCount + 1;
END CATCH

PRINT '';

-- =============================================
-- 7. TEST STRING_AGG (SQL Server 2017+)
-- =============================================
PRINT '=== 7. TEST STRING_AGG ===';

IF @VersionMajor >= 14
BEGIN
    BEGIN TRY
        ;WITH TestData AS (
            SELECT 'A' AS Valoare
            UNION ALL SELECT 'B'
            UNION ALL SELECT 'C'
        )
        SELECT STRING_AGG(Valoare, ' > ') AS TestStringAgg
        FROM TestData;
        
        PRINT '? STRING_AGG func?ioneaz? corect';
    END TRY
    BEGIN CATCH
        PRINT '? Eroare STRING_AGG: ' + ERROR_MESSAGE();
        SET @ErrorCount = @ErrorCount + 1;
    END CATCH
END
ELSE
BEGIN
    -- Test alternativa FOR XML PATH
    BEGIN TRY
        ;WITH TestData AS (
            SELECT 'A' AS Valoare
            UNION ALL SELECT 'B'
            UNION ALL SELECT 'C'
        )
        SELECT STUFF((
            SELECT ' > ' + Valoare
            FROM TestData
            FOR XML PATH('')
        ), 1, 3, '') AS TestStringAggAlternative;
        
        PRINT '? Alternativa FOR XML PATH pentru STRING_AGG func?ioneaz?';
    END TRY
    BEGIN CATCH
        PRINT '? Eroare alternativa FOR XML PATH: ' + ERROR_MESSAGE();
        SET @ErrorCount = @ErrorCount + 1;
    END CATCH
END

PRINT '';

-- =============================================
-- 8. TEST STORED PROCEDURE SYNTAX
-- =============================================
PRINT '=== 8. TEST STORED PROCEDURE ===';

BEGIN TRY
    -- Creeaz? o procedur? de test
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TestCompatibilitate]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[sp_TestCompatibilitate];
    
    DECLARE @TestProcSQL NVARCHAR(MAX) = N'
    CREATE PROCEDURE [dbo].[sp_TestCompatibilitate]
        @InputParam NVARCHAR(200),
        @OutputCount INT OUTPUT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        ;WITH TestCTE AS (
            SELECT CAST(@InputParam AS NVARCHAR(200)) AS TestValue
        )
        SELECT @OutputCount = COUNT(*) FROM TestCTE;
        
        SELECT TestValue FROM TestCTE;
    END';
    
    EXEC sp_executesql @TestProcSQL;
    
    -- Testeaz? procedura
    DECLARE @TestOutput INT;
    EXEC sp_TestCompatibilitate @InputParam = N'TestValue', @OutputCount = @TestOutput OUTPUT;
    
    IF @TestOutput = 1
        PRINT '? Stored procedures cu CTE func?ioneaz? corect';
    ELSE
    BEGIN
        PRINT '? Probleme cu stored procedures';
        SET @ErrorCount = @ErrorCount + 1;
    END
    
    -- Cleanup
    DROP PROCEDURE [dbo].[sp_TestCompatibilitate];
    
END TRY
BEGIN CATCH
    PRINT '? Eroare stored procedure: ' + ERROR_MESSAGE();
    SET @ErrorCount = @ErrorCount + 1;
END CATCH

PRINT '';

-- =============================================
-- 9. TEST PERFORMAN?? CTE COMPLEX
-- =============================================
PRINT '=== 9. TEST PERFORMAN?? ===';

BEGIN TRY
    DECLARE @StartTime DATETIME2 = GETDATE();
    
    ;WITH ComplexCTE AS (
        -- Anchor
        SELECT 
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS ID,
            CAST('Level_0_Item_' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS NVARCHAR(10)) AS NVARCHAR(200)) AS Nume,
            0 AS Nivel,
            CAST('Level_0_Item_' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS NVARCHAR(10)) AS NVARCHAR(1000)) AS Cale
        FROM (
            SELECT 1 AS x UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
        ) t1
        
        UNION ALL
        
        -- Recursive part
        SELECT 
            c.ID * 10 + sub.x,
            CAST('Level_' + CAST(c.Nivel + 1 AS NVARCHAR(2)) + '_Item_' + CAST(c.ID * 10 + sub.x AS NVARCHAR(10)) AS NVARCHAR(200)),
            c.Nivel + 1,
            CAST(c.Cale + ' > Level_' + CAST(c.Nivel + 1 AS NVARCHAR(2)) + '_Item_' + CAST(c.ID * 10 + sub.x AS NVARCHAR(10)) AS NVARCHAR(1000))
        FROM ComplexCTE c
        CROSS JOIN (SELECT 1 AS x UNION ALL SELECT 2) sub
        WHERE c.Nivel < 2
    )
    SELECT COUNT(*) AS TotalItems 
    FROM ComplexCTE
    OPTION (MAXRECURSION 100);
    
    DECLARE @EndTime DATETIME2 = GETDATE();
    DECLARE @DurationMs INT = DATEDIFF(MILLISECOND, @StartTime, @EndTime);
    
    PRINT '? CTE complex executat în ' + CAST(@DurationMs AS NVARCHAR(10)) + ' ms';
    
    IF @DurationMs > 5000  -- Mai mult de 5 secunde
    BEGIN
        PRINT '? Performance warning: CTE complex a luat mai mult de 5 secunde';
        SET @WarningCount = @WarningCount + 1;
    END
    
END TRY
BEGIN CATCH
    PRINT '? Eroare performan?? CTE: ' + ERROR_MESSAGE();
    SET @ErrorCount = @ErrorCount + 1;
END CATCH

PRINT '';

-- =============================================
-- 10. REZULTAT FINAL
-- =============================================
PRINT '================================================';
PRINT '              REZULTAT FINAL';
PRINT '================================================';

IF @ErrorCount = 0 AND @WarningCount = 0
BEGIN
    PRINT '?? COMPATIBILITATE COMPLET?: Toate testele au trecut cu succes!';
    PRINT '   SQL Server este complet compatibil cu scripturile de departamente.';
END
ELSE IF @ErrorCount = 0 AND @WarningCount > 0
BEGIN
    PRINT '?? COMPATIBILITATE CU ATEN?ION?RI: ' + CAST(@WarningCount AS NVARCHAR(10)) + ' avertismente';
    PRINT '   SQL Server este compatibil, dar unele func?ii vor folosi alternative.';
END
ELSE
BEGIN
    PRINT '?? PROBLEME DE COMPATIBILITATE: ' + CAST(@ErrorCount AS NVARCHAR(10)) + ' erori, ' + CAST(@WarningCount AS NVARCHAR(10)) + ' avertismente';
    PRINT '   SQL Server necesit? actualiz?ri sau modific?ri în scripturi.';
END

PRINT '';
PRINT 'Erori: ' + CAST(@ErrorCount AS NVARCHAR(10));
PRINT 'Avertismente: ' + CAST(@WarningCount AS NVARCHAR(10));
PRINT '';
PRINT 'Pentru detalii despre rezolvarea problemelor, consult? GHID_REZOLVARE_ERORI.md';
PRINT '================================================';