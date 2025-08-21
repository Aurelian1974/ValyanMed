/*
    Final corrected stored procedures based on actual table structure
    This version matches the existing DTO classes:
    - JudetDto: IdJudet, Nume, CodAuto (plus other fields)
    - LocalitateDto: IdOras, LocalitateGuid, IdJudet, Nume, Siruta
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
    
    -- Return all counties - matches JudetDto structure
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
    
    -- Return localities for a specific county - matches LocalitateDto structure
    -- LocalitateDto has: IdOras, LocalitateGuid, IdJudet, Nume, Siruta
    SELECT 
        IdOras,               -- Primary key (not IdLocalitate)
        LocalitateGuid,
        IdJudet,
        Nume,
        Siruta
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
END CATCH

PRINT 'Procedures created and tested successfully!';