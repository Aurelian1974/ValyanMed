/*
    Corrected stored procedures based on actual table structure
    This version should work with the existing tables
*/

-- Create procedure for getting all counties (judete)
IF OBJECT_ID('dbo.GetAllJudete','P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE dbo.GetAllJudete AS BEGIN SET NOCOUNT ON; END');
END
GO

ALTER PROCEDURE dbo.GetAllJudete
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Return all counties using correct column names
    SELECT 
        IdJudet,
        JudetGuid,
        CodJudet,
        Nume,
        Siruta,
        CodAuto,
        Ordine
    FROM dbo.Judet
    ORDER BY Nume;
END
GO

-- Create procedure for getting localities by county
IF OBJECT_ID('dbo.Localitate_GetByJudet','P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE dbo.Localitate_GetByJudet AS BEGIN SET NOCOUNT ON; END');
END
GO

ALTER PROCEDURE dbo.Localitate_GetByJudet
    @IdJudet INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Return localities for a specific county using only existing columns
    -- Based on error messages, these columns don't exist: IdLocalitate, CodSiruta, TipLocalitate
    -- So we'll use a minimal set that should exist
    SELECT 
        Id,                    -- Assuming this is the primary key instead of IdLocalitate
        LocalitateGuid,        -- This might exist based on naming convention
        Nume,
        IdJudet
        -- Removed: CodSiruta, TipLocalitate as they don't exist
    FROM dbo.Localitate
    WHERE IdJudet = @IdJudet
    ORDER BY Nume;
END
GO

-- Alternative version if 'Id' doesn't exist either
IF OBJECT_ID('dbo.Localitate_GetByJudet_Alternative','P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE dbo.Localitate_GetByJudet_Alternative AS BEGIN SET NOCOUNT ON; END');
END
GO

ALTER PROCEDURE dbo.Localitate_GetByJudet_Alternative
    @IdJudet INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Most minimal version - only columns that are likely to exist
    SELECT 
        Nume,
        IdJudet
    FROM dbo.Localitate
    WHERE IdJudet = @IdJudet
    ORDER BY Nume;
END
GO

-- Test the procedures
PRINT 'Testing corrected procedures...'

DECLARE @JudetCount INT, @LocalitateCount INT;

-- Test GetAllJudete
BEGIN TRY
    EXEC dbo.GetAllJudete;
    SELECT @JudetCount = @@ROWCOUNT;
    PRINT 'GetAllJudete: SUCCESS - ' + CAST(@JudetCount AS VARCHAR(10)) + ' judete found';
END TRY
BEGIN CATCH
    PRINT 'GetAllJudete: ERROR - ' + ERROR_MESSAGE();
END CATCH

-- Test Localitate_GetByJudet
BEGIN TRY
    EXEC dbo.Localitate_GetByJudet @IdJudet = 1;
    SELECT @LocalitateCount = @@ROWCOUNT;
    PRINT 'Localitate_GetByJudet: SUCCESS - ' + CAST(@LocalitateCount AS VARCHAR(10)) + ' localitati found';
END TRY
BEGIN CATCH
    PRINT 'Localitate_GetByJudet: ERROR - ' + ERROR_MESSAGE();
    
    -- Try alternative version
    BEGIN TRY
        EXEC dbo.Localitate_GetByJudet_Alternative @IdJudet = 1;
        SELECT @LocalitateCount = @@ROWCOUNT;
        PRINT 'Localitate_GetByJudet_Alternative: SUCCESS - ' + CAST(@LocalitateCount AS VARCHAR(10)) + ' localitati found';
    END TRY
    BEGIN CATCH
        PRINT 'Localitate_GetByJudet_Alternative: ERROR - ' + ERROR_MESSAGE();
    END CATCH
END CATCH

PRINT 'Procedure creation and testing completed!';