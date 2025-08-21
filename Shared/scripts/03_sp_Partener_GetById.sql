/*
    Stored Procedure: dbo.sp_Partener_GetById
    Purpose: Retrieve a specific partner by ID
    Usage: EXEC dbo.sp_Partener_GetById @PartenerId = 1
*/
GO
IF OBJECT_ID('dbo.sp_Partener_GetById','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Partener_GetById AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE dbo.sp_Partener_GetById
    @PartenerId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PartenerId,
        PartenerGuid,
        CodIntern,
        Denumire,
        CodFiscal,
        Judet,
        Localitate,
        Adresa,
        DataCreare,
        DataActualizare,
        UtilizatorCreare,
        UtilizatorActualizare,
        Activ
    FROM dbo.Partener WITH (NOLOCK)
    WHERE PartenerId = @PartenerId;
END
GO